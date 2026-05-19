using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Helper;

namespace ReMarkableRemember.Tests;

[TestFixture]
public sealed class OptimizedListTests
{
    [Test]
    public void Constructor_Default_StartsEmpty()
    {
        OptimizedList<Int32> list = new OptimizedList<Int32>();

        list.Count.Should().Be(0);
    }

    [Test]
    public void Constructor_WithItems_PopulatesList()
    {
        OptimizedList<Int32> list = new OptimizedList<Int32>(new List<Int32> { 1, 2, 3 });

        list.Count.Should().Be(3);
        list[0].Should().Be(1);
        list[2].Should().Be(3);
    }

    [Test]
    public void Add_RaisesCollectionChangedWithAddAction()
    {
        OptimizedList<Int32> list = new OptimizedList<Int32>();
        NotifyCollectionChangedAction? lastAction = null;
        list.CollectionChanged += (_, args) => lastAction = args.Action;

        list.Add(42);

        lastAction.Should().Be(NotifyCollectionChangedAction.Add);
        list[0].Should().Be(42);
    }

    [Test]
    public void AddRange_NonEmpty_RaisesSingleNotification()
    {
        OptimizedList<Int32> list = new OptimizedList<Int32>();
        Int32 notifications = 0;
        list.CollectionChanged += (_, _) => notifications++;

        list.AddRange(new List<Int32> { 1, 2, 3 });

        notifications.Should().Be(1, "AddRange batches its notification");
        list.Count.Should().Be(3);
    }

    [Test]
    public void AddRange_Empty_DoesNotRaiseNotification()
    {
        OptimizedList<Int32> list = new OptimizedList<Int32>();
        Int32 notifications = 0;
        list.CollectionChanged += (_, _) => notifications++;

        list.AddRange(new List<Int32>());

        notifications.Should().Be(0);
    }

    [Test]
    public void Remove_ExistingItem_RemovesAndRaisesEvent()
    {
        OptimizedList<Int32> list = new OptimizedList<Int32> { 1, 2, 3 };
        NotifyCollectionChangedAction? lastAction = null;
        list.CollectionChanged += (_, args) => lastAction = args.Action;

        Boolean result = list.Remove(2);

        result.Should().BeTrue();
        lastAction.Should().Be(NotifyCollectionChangedAction.Remove);
        list.Should().Equal(1, 3);
    }

    [Test]
    public void Remove_NonExistentItem_ReturnsFalseAndDoesNotRaise()
    {
        OptimizedList<Int32> list = new OptimizedList<Int32> { 1, 2 };
        Int32 notifications = 0;
        list.CollectionChanged += (_, _) => notifications++;

        Boolean result = list.Remove(99);

        result.Should().BeFalse();
        notifications.Should().Be(0);
    }

    [Test]
    public void Indexer_Set_ReplacesItemAndRaisesReplaceEvent()
    {
        OptimizedList<Int32> list = new OptimizedList<Int32> { 10, 20 };
        NotifyCollectionChangedAction? lastAction = null;
        list.CollectionChanged += (_, args) => lastAction = args.Action;

        list[0] = 99;

        list[0].Should().Be(99);
        lastAction.Should().Be(NotifyCollectionChangedAction.Replace);
    }

    [Test]
    public void Clear_RemovesAll()
    {
        OptimizedList<Int32> list = new OptimizedList<Int32> { 1, 2, 3 };
        NotifyCollectionChangedAction? lastAction = null;
        list.CollectionChanged += (_, args) => lastAction = args.Action;

        list.Clear();

        list.Count.Should().Be(0);
        lastAction.Should().Be(NotifyCollectionChangedAction.Remove);
    }

    [Test]
    public void Insert_AddsAtSpecificIndex()
    {
        OptimizedList<Int32> list = new OptimizedList<Int32> { 1, 3 };

        list.Insert(1, 2);

        list.Should().Equal(1, 2, 3);
    }

    [Test]
    public void RemoveRange_RemovesEachItem()
    {
        OptimizedList<Int32> list = new OptimizedList<Int32> { 1, 2, 3, 4 };

        list.RemoveRange(new List<Int32> { 2, 3 });

        list.Should().Equal(1, 4);
    }
}
