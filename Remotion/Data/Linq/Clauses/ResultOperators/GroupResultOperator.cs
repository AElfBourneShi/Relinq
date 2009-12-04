// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the group part of a query, grouping items given by an <see cref="ElementSelector"/> according to some key retrieved by a
  /// <see cref="KeySelector"/>. This is a result operator, operating on the whole result set of the query.
  /// </summary>
  /// <example>
  /// In C#, the "group by" clause in the following sample corresponds to a <see cref="GroupResultOperator"/>. "s" (a reference to the query source "s", see
  /// <see cref="QuerySourceReferenceExpression"/>) is the <see cref="ElementSelector"/> expression, "s.Country" is the <see cref="KeySelector"/>
  /// expression:
  /// <code>
  /// var query = from s in Students
  ///             where s.First == "Hugo"
  ///             group s by s.Country;
  /// </code>
  /// </example>
  public class GroupResultOperator : SequenceFromSequenceResultOperatorBase, IQuerySource
  {
    private string _itemName;

    private Expression _keySelector;
    private Expression _elementSelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupResultOperator"/> class.
    /// </summary>
    /// <param name="itemName">A name associated with the <see cref="IGrouping{TKey,TElement}"/> items generated by the result operator.</param>
    /// <param name="keySelector">The selector retrieving the key by which to group items.</param>
    /// <param name="elementSelector">The selector retrieving the elements to group.</param>
    public GroupResultOperator (string itemName, Expression keySelector, Expression elementSelector)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("itemName", itemName);
      ArgumentUtility.CheckNotNull ("keySelector", keySelector);
      ArgumentUtility.CheckNotNull ("elementSelector", elementSelector);

      _itemName = itemName;
      _elementSelector = elementSelector;
      _keySelector = keySelector;
    }

    /// <summary>
    /// Gets or sets the name of the items generated by this <see cref="GroupResultOperator"/>.
    /// </summary>
    public string ItemName
    {
      get { return _itemName; }
      set { _itemName = ArgumentUtility.CheckNotNullOrEmpty ("value", value); }
    }

    /// <summary>
    /// Gets or sets the type of the items generated by this <see cref="GroupResultOperator"/>. The item type is an instantiation of 
    /// <see cref="IGrouping{TKey,TElement}"/> derived from the types of <see cref="KeySelector"/> and <see cref="ElementSelector"/>.
    /// </summary>
    public Type ItemType
    {
      get { return typeof (IGrouping<,>).MakeGenericType (KeySelector.Type, ElementSelector.Type); }
    }

    /// <summary>
    /// Gets or sets the selector retrieving the key by which to group items.
    /// </summary>
    /// <value>The key selector.</value>
    public Expression KeySelector
    {
      get { return _keySelector; }
      set { _keySelector = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Gets or sets the selector retrieving the elements to group.
    /// </summary>
    /// <value>The element selector.</value>
    public Expression ElementSelector
    {
      get { return _elementSelector; }
      set { _elementSelector = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Clones this clause, adjusting all <see cref="QuerySourceReferenceExpression"/> instances held by it as defined by
    /// <paramref name="cloneContext"/>.
    /// </summary>
    /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext"/>.</param>
    /// <returns>A clone of this clause.</returns>
    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var clone = new GroupResultOperator (ItemName, KeySelector, ElementSelector);
      return clone;
    }

    /// <summary>
    /// Transforms all the expressions in this clause and its child objects via the given <paramref name="transformation"/> delegate.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this
    /// clause, and those expressions will be replaced with what the delegate returns.</param>
    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);

      KeySelector = transformation (KeySelector);
      ElementSelector = transformation (ElementSelector);
    }

    public override StreamedSequence ExecuteInMemory<TInput> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var executeMethod = typeof (GroupResultOperator).GetMethod ("ExecuteGroupingInMemory");
      var closedExecuteMethod = executeMethod.MakeGenericMethod (
          typeof (TInput),
          KeySelector.Type,
          ElementSelector.Type);

      return (StreamedSequence) InvokeExecuteMethod (closedExecuteMethod, input);
    }

    public StreamedSequence ExecuteGroupingInMemory<TSource, TKey, TElement> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var inputSequence = input.GetTypedSequence<TSource>();

      var keySelectorLambda = ReverseResolvingExpressionTreeVisitor.ReverseResolve (input.DataInfo.ItemExpression, KeySelector);
      var keySelector = (Func<TSource, TKey>) keySelectorLambda.Compile ();

      var elementSelectorLambda = ReverseResolvingExpressionTreeVisitor.ReverseResolve (input.DataInfo.ItemExpression, ElementSelector);
      var elementSelector = (Func<TSource, TElement>) elementSelectorLambda.Compile ();

      var resultSequence = inputSequence.GroupBy (keySelector, elementSelector);
      return new StreamedSequence (resultSequence.AsQueryable(), (StreamedSequenceInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);
      return new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (ItemType), GetNewItemExpression());
    }

    public override string ToString ()
    {
      return string.Format (
          "GroupBy({0}, {1})", 
          FormattingExpressionTreeVisitor.Format (KeySelector), 
          FormattingExpressionTreeVisitor.Format (ElementSelector));
    }

    private QuerySourceReferenceExpression GetNewItemExpression ()
    {
      return new QuerySourceReferenceExpression (this);
    }

  }
}
