using System.Collections.Generic;
using System.Diagnostics;
using UIForia;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TreeTest : MonoBehaviour {

    public class TreeNode {

        public TreeNode parent;
        public List<TreeNode> children;
        public int id;
        public int depth;

        public bool IsDescendentOf(TreeNode other) {
            TreeNode ptr = parent;
            while (ptr != null) {
                if (ptr == other) {
                    return true;
                }

                ptr = ptr.parent;
            }

            return false;
        }

    }

    public class TreeGenerator {

        public int id;

        public TreeNode CreateTree(int depth, TreeNode parent, int maxDepth, int childCount, List<TreeNode> flatList) {
            var node = new TreeNode();
            node.depth = depth;
            flatList.Add(node);
            node.id = id++;
            node.parent = parent;
            if (maxDepth > 0) {
                node.children = new List<TreeNode>(childCount);
                for (int i = 0; i < childCount; ++i) {
                    node.children.Add(CreateTree(depth + 1, node, maxDepth - 1, childCount, flatList));
                }
            }
            else {
                node.children = new List<TreeNode>(0);
            }

            return node;
        }

    }

    // Start is called before the first frame update
    public TreeNode tree;

    public struct TestJobSchedule : IJob {

        public void Execute() {
          
        }

    }

    void Start() {

        int count = 1000;

        NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(count, Allocator.Persistent);

        for (int i = 0; i < count - 1; i++) {
            jobs[i] = new TestJobSchedule().Schedule(jobs[i + 1]);
        }

        jobs[count - 1] = new TestJobSchedule().Schedule(); //jobs[1]);

        Stopwatch stop = Stopwatch.StartNew();
        JobHandle.ScheduleBatchedJobs();
        Debug.Log("time: " + stop.Elapsed.TotalMilliseconds.ToString("F3"));
        JobHandle.CompleteAll(jobs);
        

        for (int i = 0; i < count - 1; i++) {
            jobs[i] = new TestJobSchedule().Schedule(jobs[i + 1]);
        }

        jobs[count - 1] = new TestJobSchedule().Schedule(); //jobs[1]);

        Stopwatch stop2= Stopwatch.StartNew();
        JobHandle.ScheduleBatchedJobs();
        Debug.Log("time: " + stop2.Elapsed.TotalMilliseconds.ToString("F3"));
        JobHandle.CompleteAll(jobs);

        jobs.Dispose();
        return;
        List<TreeNode> nodes = new List<TreeNode>(10000);
        TreeGenerator generator = new TreeGenerator();
        tree = generator.CreateTree(0, null, 6, 5, nodes);
        Debug.Log(nodes.Count + " nodes");
        BufferList<bool> expectedResults = new BufferList<bool>(nodes.Count * nodes.Count, Allocator.Persistent);

        for (int i = 0; i < nodes.Count; i++) {
            for (int j = 0; j < nodes.Count; j++) {
                nodes[j].IsDescendentOf(nodes[i]);
            }
        }

        Stopwatch s = Stopwatch.StartNew();

        int x = 0;
        for (int i = 0; i < nodes.Count; i++) {
            for (int j = 0; j < nodes.Count; j++) {
                expectedResults[x++] = (nodes[j].IsDescendentOf(nodes[i]));
            }
        }

        expectedResults.size = nodes.Count * nodes.Count;

        Debug.Log("traditional method took : " + s.Elapsed.TotalMilliseconds.ToString("F4"));

        BufferList<ElementTraversalInfo> traversalInfo = new BufferList<ElementTraversalInfo>(nodes.Count, Allocator.Persistent);
        BufferList<bool> results = new BufferList<bool>(nodes.Count * nodes.Count, Allocator.Persistent);

        TraversalIndexJob_Managed job = new TraversalIndexJob_Managed() {
            root = tree,
            stack = new LightStack<TreeNode>(nodes.Count / 2)
        };
        job.Execute(ref traversalInfo);
        Stopwatch s2 = Stopwatch.StartNew();
        job.Execute(ref traversalInfo);
        Debug.Log("traversal took : " + s2.Elapsed.TotalMilliseconds.ToString("F4"));

        traversalInfo.size = nodes.Count;

        new TestJob() {
            traversalInfo = traversalInfo,
            output = results
        }.Run();

        results.size = 0;

        new TestJob() {
            traversalInfo = traversalInfo,
            output = results
        }.Run();

        results.size = 0;

        Stopwatch s3 = Stopwatch.StartNew();

        new TestJob() {
            traversalInfo = traversalInfo,
            output = results
        }.Run();
        Debug.Log("query took " + s3.Elapsed.TotalMilliseconds.ToString("F4"));

        for (int i = 0; i < expectedResults.size; i++) {
            if (results[i] != expectedResults[i]) {
                Debug.Log("wrong " + i);
                break;
            }
        }
        // for (int i = 0; i < traversalInfo.size; i++) {
        //     for (int j = 0; j < traversalInfo.size; j++) {
        //         traversalInfo[i].IsDescendentOf(traversalInfo[j]);
        //         // if (expectedResults[idx++] != traversalInfo[i].IsDescendentOf(traversalInfo[j])) {
        //             // Debug.Log("wrong");
        //             // traversalInfo.Dispose();
        //             // return;
        //         // }
        //     }
        // }

        // PrintTree(tree, traversalInfo);

        traversalInfo.Dispose();
        results.Dispose();

    }

    public void PrintTree(TreeNode node, BufferList<ElementTraversalInfo> traversalInfo) {
        string retn = new string(' ', 4 * node.depth);

        retn += " id = (" + node.id + ") ";
        retn += "  ftb = " + traversalInfo[node.id].ftbIndex;
        retn += "  btf = " + traversalInfo[node.id].btfIndex;

        Debug.Log(retn);

        for (int i = 0; i < node.children.Count; i++) {
            PrintTree(node.children[i], traversalInfo);
        }

    }

    [BurstCompile(CompileSynchronously = true)]
    public unsafe struct TestJob : IJob {

        public BufferList<ElementTraversalInfo> traversalInfo;
        public BufferList<bool> output;

        public void Execute() {
            int idx = 0;
            for (int i = 0; i < traversalInfo.size; i++) {
                for (int j = 0; j < traversalInfo.size; j++) {
                    output[idx++] = traversalInfo[j].IsDescendentOf(traversalInfo[i]);
                }
            }

            output.size = idx;
        }

    }

    public unsafe struct TraversalIndexJob_Managed {

        /// <summary>
        /// freed internally
        /// </summary>
        /// <summary>
        /// output list, expects to already have proper size
        /// </summary>
        public LightStack<TreeNode> stack;

        public TreeNode root;

        public void Execute(ref BufferList<ElementTraversalInfo> traversalInfo) {

            // LightStack<TreeNode>.Get();

            stack.array[stack.size++] = root;
            int idx = 0;

            ushort ftbIndex = 0;
            ushort btfIndex = 0;

            List<int> forward = new List<int>();
            List<int> backwards = new List<int>();
            while (stack.size != 0) {

                TreeNode current = stack.array[--stack.size];

                forward.Add(current.id);

                traversalInfo.array[current.id] = new ElementTraversalInfo() {
                    depth = (ushort) current.depth,
                    ftbIndex = ftbIndex++
                };

                // current.ftbIndex = (ushort) (ftbIndex - 1);

                int childCount = current.children.Count;

                //stack.EnsureAdditionalCapacity(childCount);

                for (int i = childCount - 1; i >= 0; i--) {

                    TreeNode child = current.children[i];

                    //  if ((child.flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet) {
                    stack.array[stack.size++] = child;
                    //  }

                }

            }

            stack.array[stack.size++] = root;
            idx = 0;

            while (stack.size != 0) {

                TreeNode current = stack.array[--stack.size];
                backwards.Add(current.id);
                traversalInfo.array[current.id].btfIndex = btfIndex++;

                int childCount = current.children.Count; //ChildCount;
                // current.btfIndex = (ushort) (btfIndex - 1);

                for (int i = 0; i < childCount; i++) {

                    TreeNode child = current.children[i];

                    stack.array[stack.size++] = child;

                }

            }

            traversalInfo.size = idx;
            // stack.Release();
            // rootElementHandle.Free();
        }

    }

    // Update is called once per frame
    void Update() { }

}