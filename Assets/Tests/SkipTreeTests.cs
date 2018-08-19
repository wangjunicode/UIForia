using System.Diagnostics;
using NUnit.Framework;
using Src;

[TestFixture]
public class SkipTreeTests {

    [DebuggerDisplay("{name}")]
    public class Item : ISkipTreeTraversable {

        public Item parent;
        public string name;

        public Item(Item parent = null, string name = null) {
            this.parent = parent;
            this.name = name;
            if (this.name == null) {
                this.name = "__NO_NAME_";
            }
        }

        public IHierarchical Element => this;
        public IHierarchical Parent => parent;

        public void OnParentChanged(ISkipTreeTraversable newParent) {
        }

        public void OnBeforeTraverse() {
            throw new System.NotImplementedException();
        }

        public void OnAfterTraverse() {
            throw new System.NotImplementedException();
        }

    }

    [Test]
    public void AddNodeToRoot() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(one, "two");
        var three = new Item(one, "three");
        tree.AddItem(two);
        tree.AddItem(three);
        string[] output = new string[2];
        int i = 0;
        tree.TraversePreOrderWithCallback((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "two", "three" }, output);
    }

    [Test]
    public void AddParentAfterChildren() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(one, "two");
        var three = new Item(one, "three");
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(one);
        string[] output = new string[3];
        int i = 0;
        tree.TraversePreOrderWithCallback((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "two", "three" }, output);
    }

    [Test]
    public void MissingParentInTree() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(one, "two");
        var three = new Item(two, "three");
        var four = new Item(two, "four");
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(one);
        string[] output = new string[3];
        int i = 0;
        tree.TraversePreOrderWithCallback((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "three", "four" }, output);
    }

    [Test]
    public void RemoveAnElement() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(one, "two");
        var three = new Item(one, "three");
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(one);
        tree.RemoveItem(one);
        string[] output = new string[2];
        int i = 0;
        tree.TraversePreOrderWithCallback((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "two", "three" }, output);
    }
    
    [Test]
    public void RemoveAnElementWithSiblings() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        tree.RemoveItem(two);
        string[] output = new string[5];
        int i = 0;
        tree.TraversePreOrderWithCallback((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "four", "five", "six", "three" }, output);
        Assert.AreEqual(tree.Size, 5);
    }
    
    [Test]
    public void RemoveAnElementHierarchy() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        tree.RemoveHierarchy(two);
        string[] output = new string[2];
        int i = 0;
        tree.TraversePreOrderWithCallback((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "three" }, output);
        Assert.AreEqual(2, tree.Size);
    }

    [Test]
    public void DisableHierarchy() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        tree.DisableHierarchy(two);
        string[] output = new string[2];
        int i = 0;
        tree.TraversePreOrderWithCallback((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "three" }, output);
        Assert.AreEqual(6, tree.Size);
    }

    [Test]
    public void EnableHierarchy() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        tree.DisableHierarchy(two);
        tree.EnableHierarchy(two);
        string[] output = new string[6];
        int i = 0;
        tree.TraversePreOrderWithCallback((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "two", "four", "five", "six", "three" }, output);
        Assert.AreEqual(6, tree.Size);
    }
    
}