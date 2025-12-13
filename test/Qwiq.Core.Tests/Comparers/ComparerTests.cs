using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qwiq.Mocks;
using Qwiq.Tests.Common;
using Shouldly;

namespace Qwiq.Comparers
{
    /// <summary>
    /// Tests for NullableIdentifiableComparer.
    /// </summary>
    [TestClass]
    public class Given_NullableIdentifiableComparer_with_two_items_same_id : ContextSpecification
    {
        private bool _result;
        private MockWorkItem _item1 = null!;
        private MockWorkItem _item2 = null!;

        public override void Given()
        {
            var workItemType = new MockWorkItemType("Bug");
            _item1 = new MockWorkItem(workItemType, 123);
            _item2 = new MockWorkItem(workItemType, 123);
        }

        public override void When()
        {
            _result = NullableIdentifiableComparer.Default.Equals(_item1, _item2);
        }

        [TestMethod]
        public void Then_returns_true()
        {
            _result.ShouldBeTrue();
        }
    }

    [TestClass]
    public class Given_NullableIdentifiableComparer_with_two_items_different_id : ContextSpecification
    {
        private bool _result;
        private MockWorkItem _item1 = null!;
        private MockWorkItem _item2 = null!;

        public override void Given()
        {
            var workItemType = new MockWorkItemType("Bug");
            _item1 = new MockWorkItem(workItemType, 123);
            _item2 = new MockWorkItem(workItemType, 456);
        }

        public override void When()
        {
            _result = NullableIdentifiableComparer.Default.Equals(_item1, _item2);
        }

        [TestMethod]
        public void Then_returns_false()
        {
            _result.ShouldBeFalse();
        }
    }

    [TestClass]
    public class Given_NullableIdentifiableComparer_with_null_items : ContextSpecification
    {
        private MockWorkItem _item = null!;

        public override void Given()
        {
            var workItemType = new MockWorkItemType("Bug");
            _item = new MockWorkItem(workItemType, 123);
        }

        [TestMethod]
        public void Then_null_null_returns_true()
        {
            NullableIdentifiableComparer.Default.Equals(null, null).ShouldBeTrue();
        }

        [TestMethod]
        public void Then_item_null_returns_false()
        {
            NullableIdentifiableComparer.Default.Equals(_item, null).ShouldBeFalse();
        }

        [TestMethod]
        public void Then_null_item_returns_false()
        {
            NullableIdentifiableComparer.Default.Equals(null, _item).ShouldBeFalse();
        }
    }

    [TestClass]
    public class Given_NullableIdentifiableComparer_GetHashCode : ContextSpecification
    {
        private MockWorkItem _item = null!;

        public override void Given()
        {
            var workItemType = new MockWorkItemType("Bug");
            _item = new MockWorkItem(workItemType, 123);
        }

        [TestMethod]
        public void Then_null_returns_zero()
        {
            NullableIdentifiableComparer.Default.GetHashCode(null!).ShouldBe(0);
        }

        [TestMethod]
        public void Then_item_returns_non_zero_hashcode()
        {
            NullableIdentifiableComparer.Default.GetHashCode(_item).ShouldNotBe(0);
        }
    }

    /// <summary>
    /// Tests for IdentifiableComparer.
    /// </summary>
    [TestClass]
    public class Given_IdentifiableComparer_with_two_items_same_id : ContextSpecification
    {
        private bool _result;
        private MockWorkItem _item1 = null!;
        private MockWorkItem _item2 = null!;

        public override void Given()
        {
            var workItemType = new MockWorkItemType("Bug");
            _item1 = new MockWorkItem(workItemType, 456);
            _item2 = new MockWorkItem(workItemType, 456);
        }

        public override void When()
        {
            _result = IdentifiableComparer.Default.Equals(_item1, _item2);
        }

        [TestMethod]
        public void Then_returns_true()
        {
            _result.ShouldBeTrue();
        }
    }

    /// <summary>
    /// Tests for WorkItemComparer.
    /// </summary>
    [TestClass]
    public class Given_WorkItemComparer_with_same_work_items : ContextSpecification
    {
        private bool _result;
        private MockWorkItem _wi1 = null!;
        private MockWorkItem _wi2 = null!;

        public override void Given()
        {
            var workItemType = new MockWorkItemType("Bug");
            _wi1 = new MockWorkItem(workItemType, 100);
            _wi2 = new MockWorkItem(workItemType, 100);
        }

        public override void When()
        {
            _result = WorkItemComparer.Default.Equals(_wi1, _wi2);
        }

        [TestMethod]
        public void Then_returns_true()
        {
            _result.ShouldBeTrue();
        }
    }

    [TestClass]
    public class Given_WorkItemComparer_with_different_ids : ContextSpecification
    {
        private bool _result;
        private MockWorkItem _wi1 = null!;
        private MockWorkItem _wi2 = null!;

        public override void Given()
        {
            var workItemType = new MockWorkItemType("Bug");
            _wi1 = new MockWorkItem(workItemType, 100);
            _wi2 = new MockWorkItem(workItemType, 200);
        }

        public override void When()
        {
            _result = WorkItemComparer.Default.Equals(_wi1, _wi2);
        }

        [TestMethod]
        public void Then_returns_false()
        {
            _result.ShouldBeFalse();
        }
    }

    /// <summary>
    /// Tests for WorkItemTypeComparer.
    /// </summary>
    [TestClass]
    public class Given_WorkItemTypeComparer_with_same_types : ContextSpecification
    {
        private bool _result;

        public override void When()
        {
            var type1 = new MockWorkItemType("Bug", "Test Project");
            var type2 = new MockWorkItemType("Bug", "Test Project");
            _result = WorkItemTypeComparer.Default.Equals(type1, type2);
        }

        [TestMethod]
        public void Then_returns_true()
        {
            _result.ShouldBeTrue();
        }
    }

    [TestClass]
    public class Given_WorkItemTypeComparer_with_different_names : ContextSpecification
    {
        private bool _result;

        public override void When()
        {
            var type1 = new MockWorkItemType("Bug", "Test Project");
            var type2 = new MockWorkItemType("Task", "Test Project");
            _result = WorkItemTypeComparer.Default.Equals(type1, type2);
        }

        [TestMethod]
        public void Then_returns_false()
        {
            _result.ShouldBeFalse();
        }
    }

    /// <summary>
    /// Tests for GenericComparer.
    /// </summary>
    [TestClass]
    public class Given_GenericComparer_with_string_items : ContextSpecification
    {
        private bool _result;

        public override void When()
        {
            _result = GenericComparer<string>.Default.Equals("test", "test");
        }

        [TestMethod]
        public void Then_returns_true()
        {
            _result.ShouldBeTrue();
        }
    }

    [TestClass]
    public class Given_GenericComparer_with_different_strings : ContextSpecification
    {
        private bool _result;

        public override void When()
        {
            _result = GenericComparer<string>.Default.Equals("test1", "test2");
        }

        [TestMethod]
        public void Then_returns_false()
        {
            _result.ShouldBeFalse();
        }
    }

    [TestClass]
    public class Given_GenericComparer_with_null : ContextSpecification
    {
        [TestMethod]
        public void Then_null_null_returns_true()
        {
            GenericComparer<string>.Default.Equals(null, null).ShouldBeTrue();
        }

        [TestMethod]
        public void Then_value_null_returns_false()
        {
            GenericComparer<string>.Default.Equals("test", null).ShouldBeFalse();
        }

        [TestMethod]
        public void Then_GetHashCode_null_returns_zero()
        {
            GenericComparer<string>.Default.GetHashCode(null!).ShouldBe(0);
        }
    }
}
