﻿using Docdown.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Docdown.Controls
{
    /// <summary>
    /// The <see cref="TreeGrid"/> works like the the usual IDE grid.
    /// </summary>
    public class TreeGrid : Panel
    {
        public static readonly DependencyProperty TreeProperty 
            = DependencyProperty.Register(nameof(Tree), typeof(TreeNode), typeof(TreeGrid), new PropertyMetadata(null, TreeChanged));
        public static readonly DependencyProperty SplitterThicknessProperty 
            = DependencyProperty.Register(nameof(SplitterThickness), typeof(double), typeof(TreeGrid));

        public static readonly DependencyProperty TreeNodeProperty
            = DependencyProperty.RegisterAttached(nameof(TreeNode), typeof(TreeNode), typeof(TreeGrid));

        public static TreeNode GetTreeNode(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(TreeNodeProperty) as TreeNode;
        }

        public static void SetTreeNode(DependencyObject dependencyObject, TreeNode treeNode)
        {
            dependencyObject.SetValue(TreeNodeProperty, treeNode);
        }

        public TreeNode Tree
        {
            get => (TreeNode)GetValue(TreeProperty);
            set => SetValue(TreeProperty, value);
        }

        public double SplitterThickness
        {
            get => (double)GetValue(SplitterThicknessProperty);
            set => SetValue(SplitterThicknessProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize;

            if (Tree == null)
            {
                desiredSize = new Size(0, 0);
            }
            else
            {
                desiredSize = Tree.Measure(availableSize, SplitterThickness);
            }

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Tree != null)
            {
                var finalRect = new Rect(finalSize);

                Tree.Arrange(finalRect, SplitterThickness);
            }

            return finalSize;
        }

        private static void TreeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TreeGrid grid)
            {
                grid.Children.Clear();
                if (e.NewValue != null)
                    grid.AddNode((TreeNode)e.NewValue);

                grid.InvalidateMeasure();
            }
        }

        private void AddNode(TreeNode node)
        {
            node.Grid = this;
            if (node.Element != null)
            {
                node.Splitter = null;
                Children.Add(node.Element);
                SetTreeNode(node.Element, node);
            }
            else
            {
                AddNode(node.A);
                if (node.Splitter == null)
                {
                    node.Splitter = new TreeGridSplitter
                    {
                        Orientation = node.Orientation,
                        Node = node
                    };
                }
                Children.Add(node.Splitter);
                AddNode(node.B);
            }
        }

        public static ActionCommand RemoveTreeGridItemCommand(UIElement element)
        {
            return new ActionCommand(() =>
            {
                var treeNode = GetTreeNode(element);
                treeNode.Parent.Remove(treeNode.Element);
            });
        }
    }

    public class TreeNode
    {
        public string Id { get; set; }
        public UIElement Element { get; set; }
        public TreeNode A { get; set; }
        public TreeNode B { get; set; }
        public TreeNode Parent { get; set; }
        public TreeNode TopParent
        {
            get
            {
                if (Parent == null)
                {
                    return this;
                }
                return Parent.TopParent;
            }
        }
        public double Distribution
        {
            get => distribution;
            set
            {
                distribution = Math.Min(0.9, Math.Max(0.1, value));
                Grid?.InvalidateMeasure();
            }
        }
        public Orientation Orientation { get; set; }
        public TreeGridSplitter Splitter { get; set; }
        public TreeGrid Grid { get; set; }

        public Size Size { get; set; }

        private double distribution = 0.5;

        public void AddA(UIElement element, Orientation orientation, double distribution = 0.5)
        {
            A = new TreeNode
            {
                Element = element,
                Parent = this
            };
            B = new TreeNode
            {
                Element = Element,
                Parent = this
            };
            Orientation = orientation;
            Distribution = distribution;
            Element = null;
            Grid.Tree = null;
            Grid.Tree = TopParent;
        }

        public void RemoveA()
        {
            if (A.Element != null)
            {
                Grid.Children.Remove(A.Element);
            }

            Element = B.Element;
            TreeGrid.SetTreeNode(Element, this);
            A = B = null;
            Grid.Tree = null;
            Grid.Tree = TopParent;
        }

        public void RemoveB()
        {
            if (B.Element != null)
            {
                Grid.Children.Remove(B.Element);
            }

            Element = A.Element;
            TreeGrid.SetTreeNode(Element, this);
            A = B = null;
            Grid.Tree = null;
            Grid.Tree = TopParent;
        }

        public void Remove(UIElement element)
        {
            if (A != null && A.Element == element)
            {
                RemoveA();
            }
            else if (B != null && B.Element == element)
            {
                RemoveB();
            }
        }

        public void Remove(string id)
        {
            if (A != null && A.Id == id)
            {
                RemoveA();
            }
            else if (B != null && B.Id == id)
            {
                RemoveB();
            }
            else if (A != null)
            {
                A.Remove(id);
            }
            else if (B != null)
            {
                B.Remove(id);
            }
        }

        public void AddB(UIElement element, Orientation orientation, double distribution = 0.5)
        {
            A = new TreeNode
            {
                Element = Element,
                Parent = this
            };
            B = new TreeNode
            {
                Element = element,
                Parent = this
            };
            Orientation = orientation;
            Distribution = distribution;
            Element = null;
            Grid.Tree = null;
            Grid.Tree = TopParent;
        }

        public bool ContainsId(string id)
        {
            if (Id == id)
            {
                return true;
            }
            else if (A != null && B != null)
            {
                return A.ContainsId(id) || B.ContainsId(id);
            }
            else
            {
                return false;
            }
        }

        public Size Measure(Size availableSize, double splitterThickness)
        {
            Size = availableSize;
            if (Element != null)
            {
                Element.Measure(availableSize);
                return availableSize;
            }
            else
            {
                // default orientation is horizontal
                double width = availableSize.Width;
                double height = availableSize.Height;
                double splitter = splitterThickness;
                bool horizontal = Orientation == Orientation.Horizontal;

                Size sizeA;
                Size sizeB;
                Size splitterSize;

                if (horizontal)
                {
                    width -= splitter;
                    double widthA = width * Distribution;
                    double widthB = width * (1 - Distribution);
                    sizeA = new Size(widthA, height);
                    sizeB = new Size(widthB, height);
                    splitterSize = new Size(splitter, height);
                }
                else
                {
                    height -= splitter;
                    double heightA = height * Distribution;
                    double heightB = height * (1 - Distribution);
                    sizeA = new Size(width, heightA);
                    sizeB = new Size(width, heightB);
                    splitterSize = new Size(width, splitter);
                }

                sizeA = A.Measure(sizeA, splitterThickness);
                sizeB = B.Measure(sizeB, splitterThickness);
                Splitter.Measure(splitterSize);

                if (horizontal)
                {
                    width = sizeA.Width + sizeB.Width + splitter;
                    height = Math.Max(sizeA.Height, sizeB.Height);
                }
                else
                {
                    width = Math.Max(sizeA.Width, sizeB.Width);
                    height = sizeA.Height + sizeB.Height + splitter;
                }

                return new Size(width, height);
            }
        }

        public void Arrange(Rect rect, double splitterThickness)
        {
            if (Element != null)
            {
                Element.Arrange(rect);
            }
            else
            {
                double x = rect.X;
                double y = rect.Y;
                double width = rect.Width;
                double height = rect.Height;
                bool horizontal = Orientation == Orientation.Horizontal;

                Rect rectA;
                Rect rectB;
                Rect splitterRect;

                if (horizontal)
                {
                    width -= splitterThickness;
                    double widthA = width * Distribution;
                    double widthB = width * (1 - Distribution);
                    double xA = x;
                    double xB = x + widthA + splitterThickness;
                    double xSplitter = x + widthA;
                    rectA = new Rect(xA, y, widthA, height);
                    rectB = new Rect(xB, y, widthB, height);
                    splitterRect = new Rect(xSplitter, y, splitterThickness, height);
                }
                else
                {
                    height -= splitterThickness;
                    double heightA = height * Distribution;
                    double heightB = height * (1 - Distribution);
                    double yA = y;
                    double yB = y + heightA + splitterThickness;
                    double ySplitter = y + heightA;
                    rectA = new Rect(x, yA, width, heightA);
                    rectB = new Rect(x, yB, width, heightB);
                    splitterRect = new Rect(x, ySplitter, width, splitterThickness);
                }

                A.Arrange(rectA, splitterThickness);
                B.Arrange(rectB, splitterThickness);
                Splitter.Arrange(splitterRect);
            }
        }
    }
}
