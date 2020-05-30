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
        public ElementId id;
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
        
        public ElementTable<HierarchyInfo> hierarchyTable;
        public ElementTable<ElementTraversalInfo> traversalTable;
        
        public void AddChild(ElementId parentId, ElementId childId) {
            ref HierarchyInfo parentInfo = ref hierarchyTable[parentId];
            ref HierarchyInfo childInfo = ref hierarchyTable[childId];

            childInfo.parentId = parentId;

            if (parentInfo.childCount == 0) {
                parentInfo.firstChildId = childId;
                parentInfo.lastChildId = childId;
                childInfo.nextSiblingId = default;
                childInfo.prevSiblingId = default;
            }
            else {
                childInfo.prevSiblingId = parentInfo.lastChildId;
                childInfo.nextSiblingId = default;
                parentInfo.lastChildId = childId;
            }

            parentInfo.childCount++;
            
        }
        
        public TreeNode CreateTree(int depth, TreeNode parent, int maxDepth, int childCount, List<TreeNode> flatList) {
            var node = new TreeNode();
            node.depth = depth;
            flatList.Add(node);
            node.id = default; // todo -- new ElementId(id++, 1);
            node.parent = parent;
            if (maxDepth > 0) {
                node.children = new List<TreeNode>(childCount);
                for (int i = 0; i < childCount; ++i) {
                    node.children.Add(CreateTree(depth + 1, node, maxDepth - 1, childCount, flatList));
                    AddChild(node.id, node.children[node.children.Count - 1].id);
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


        List<TreeNode> nodes = new List<TreeNode>(10000);
        TreeGenerator generator = new TreeGenerator();
        tree = generator.CreateTree(0, null, 6, 5, nodes);
        Debug.Log(nodes.Count + " nodes");
        //BufferList<bool> expectedResults = new BufferList<bool>(nodes.Count * nodes.Count, Allocator.Persistent);
        //
        // for (int i = 0; i < nodes.Count; i++) {
        //     for (int j = 0; j < nodes.Count; j++) {
        //         nodes[j].IsDescendentOf(nodes[i]);
        //     }
        // }

        Stopwatch s = Stopwatch.StartNew();

        int x = 0;
        // for (int i = 0; i < nodes.Count; i++) {
        //     for (int j = 0; j < nodes.Count; j++) {
        //         expectedResults[x++] = (nodes[j].IsDescendentOf(nodes[i]));
        //     }
        // }
        //
        // expectedResults.size = nodes.Count * nodes.Count;

        // Debug.Log("traditional method took : " + s.Elapsed.TotalMilliseconds.ToString("F4"));

        BufferList<ElementTraversalInfo> traversalInfo = new BufferList<ElementTraversalInfo>(nodes.Count, Allocator.Persistent);
        // BufferList<bool> results = new BufferList<bool>(nodes.Count * nodes.Count, Allocator.Persistent);

        TraversalIndexJob job = new TraversalIndexJob() {
        };
        job.Run();
        Stopwatch s2 = Stopwatch.StartNew();
        job.Run();
        
        Debug.Log("traversal took : " + s2.Elapsed.TotalMilliseconds.ToString("F4"));
        traversalInfo.size = nodes.Count;
        traversalInfo.Dispose();

        //
        // new TestJob() {
        //     traversalInfo = traversalInfo,
        //     output = results
        // }.Run();
        //
        // results.size = 0;
        //
        // new TestJob() {
        //     traversalInfo = traversalInfo,
        //     output = results
        // }.Run();
        //
        // results.size = 0;
        //
        // Stopwatch s3 = Stopwatch.StartNew();
        //
        // new TestJob() {
        //     traversalInfo = traversalInfo,
        //     output = results
        // }.Run();
        // Debug.Log("query took " + s3.Elapsed.TotalMilliseconds.ToString("F4"));
        //
        // for (int i = 0; i < expectedResults.size; i++) {
        //     if (results[i] != expectedResults[i]) {
        //         Debug.Log("wrong " + i);
        //         break;
        //     }
        // }
        // // for (int i = 0; i < traversalInfo.size; i++) {
        // //     for (int j = 0; j < traversalInfo.size; j++) {
        // //         traversalInfo[i].IsDescendentOf(traversalInfo[j]);
        // //         // if (expectedResults[idx++] != traversalInfo[i].IsDescendentOf(traversalInfo[j])) {
        // //             // Debug.Log("wrong");
        // //             // traversalInfo.Dispose();
        // //             // return;
        // //         // }
        // //     }
        // // }
        //
        // // PrintTree(tree, traversalInfo);
        //
        // traversalInfo.Dispose();
        // results.Dispose();

    }

    public void PrintTree(TreeNode node, BufferList<ElementTraversalInfo> traversalInfo) {
        string retn = new string(' ', 4 * node.depth);

        // retn += " id = (" + node.id + ") ";
        // retn += "  ftb = " + traversalInfo[node.id].ftbIndex;
        // retn += "  btf = " + traversalInfo[node.id].btfIndex;

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

    [BurstCompile(CompileSynchronously = true)]
    public unsafe struct TraversalIndexJob : IJob {
        
        public ElementId rootId;
        public ElementTable<ElementTraversalInfo> traversalTable;
        public ElementTable<HierarchyInfo> hierarchyTable;
        
        public void Execute() {

            ushort ftbIndex = 0;
            ushort btfIndex = 0;

            DataList<ElementId> stack = new DataList<ElementId>(512, Allocator.TempJob);

            stack[stack.size++] = rootId;

            while (stack.size != 0) {

                ElementId current = stack[--stack.size];

                traversalTable[current].ftbIndex = ftbIndex++;

                int childCount = hierarchyTable[current].childCount;

                stack.EnsureAdditionalCapacity(childCount);

                ElementId childPtr = hierarchyTable[current].lastChildId;

                for (int i = 0; i < childCount; i++) {

                    //if (!ElementSystem.IsDeadOrDisabled(childPtr, metaTable)) {
                    stack.AddUnchecked(childPtr);
                    // }

                    childPtr = hierarchyTable[childPtr].prevSiblingId;

                }

            }

            stack[stack.size++] = rootId;

            while (stack.size != 0) {

                ElementId current = stack[--stack.size];

                traversalTable[current].btfIndex = btfIndex++;

                int childCount = hierarchyTable[current].childCount;

                ElementId childPtr = hierarchyTable[current].firstChildId;

                for (int i = 0; i < childCount; i++) {

                    //  if (!ElementSystem.IsDeadOrDisabled(childPtr, metaTable)) {
                    stack.AddUnchecked(childPtr);
                    //   }

                    childPtr = hierarchyTable[childPtr].nextSiblingId;

                }

            }

            stack.Dispose();

        }

    }

}