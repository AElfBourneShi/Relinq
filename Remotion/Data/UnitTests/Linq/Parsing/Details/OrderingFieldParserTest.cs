// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Collections;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.Parsing.Details
{
  [TestFixture]
  public class OrderingFieldParserTest
  {
    private JoinedTableContext _joinedTableContext;

    [SetUp]
    public void SetUp ()
    {
      _joinedTableContext = new JoinedTableContext();
    }

    [Test]
    public void SimpleOrderingClause ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateSimpleOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();
      Ordering ordering = orderBy.OrderingList.First ();

      OrderingFieldParser parser = new OrderingFieldParser (StubDatabaseInfo.Instance);
      FieldDescriptor fieldDescriptor = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof(Student).GetProperty("First"));
      ParseContext parseContext = new ParseContext (
          parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      Assert.AreEqual (new OrderingField (fieldDescriptor, OrderingDirection.Asc), parser.Parse (ordering.Expression.Body, parseContext, ordering.OrderingDirection));
    }

    [Test]
    public void TwoOrderingClause_FirstClause ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateTwoOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.BodyClauses.First ();
      Ordering ordering = orderBy1.OrderingList.First ();

      OrderingFieldParser parser = new OrderingFieldParser (StubDatabaseInfo.Instance);

      FieldDescriptor fieldDescriptor1 = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      ParseContext parseContext = new ParseContext (
          parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      Assert.AreEqual (new OrderingField (fieldDescriptor1, OrderingDirection.Asc), parser.Parse (ordering.Expression.Body, parseContext, ordering.OrderingDirection));
    }

    [Test]
    public void TwoOrderingClause_SecondClause ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateTwoOrderByQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy2 = (OrderByClause) parsedQuery.BodyClauses.Last ();
      Ordering ordering = orderBy2.OrderingList.Last ();

      OrderingFieldParser parser = new OrderingFieldParser (StubDatabaseInfo.Instance);

      FieldDescriptor fieldDescriptor2 = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("Last"));
      ParseContext parseContext = new ParseContext (
          parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      Assert.AreEqual (new OrderingField (fieldDescriptor2, OrderingDirection.Desc), parser.Parse (ordering.Expression.Body, parseContext, ordering.OrderingDirection));
    }

    [Test]
    public void ComplexOrderingClause_FirstOrdering ()
    {
      IQueryable<Student> query =
          MixedTestQueryGenerator.CreateMultiFromWhereOrderByQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.BodyClauses.Skip (2).First ();
      Ordering ordering = orderBy1.OrderingList.First ();

      OrderingFieldParser parser = new OrderingFieldParser (StubDatabaseInfo.Instance);
      FieldDescriptor fieldDescriptor = ExpressionHelper.CreateFieldDescriptor (parsedQuery.MainFromClause, typeof (Student).GetProperty ("First"));
      ParseContext parseContext = new ParseContext (
         parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      Assert.AreEqual (new OrderingField (fieldDescriptor, OrderingDirection.Asc), parser.Parse (ordering.Expression.Body, parseContext, ordering.OrderingDirection));
    }

    [Test]
    public void ComplexOrderingClause_SecondOrdering ()
    {
      IQueryable<Student> query =
          MixedTestQueryGenerator.CreateMultiFromWhereOrderByQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy1 = (OrderByClause) parsedQuery.BodyClauses.Skip (2).First ();
      Ordering ordering = orderBy1.OrderingList.Last ();

      OrderingFieldParser parser = new OrderingFieldParser (StubDatabaseInfo.Instance);
      FieldDescriptor fieldDescriptor = ExpressionHelper.CreateFieldDescriptor ((FromClauseBase) parsedQuery.BodyClauses[0],
          typeof (Student).GetProperty ("Last"));
      ParseContext parseContext = new ParseContext (
          parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      Assert.AreEqual (new OrderingField (fieldDescriptor, OrderingDirection.Desc), parser.Parse (ordering.Expression.Body, parseContext, ordering.OrderingDirection));
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Remotion.Data.UnitTests.Linq.Student.NonDBProperty' "
        +"does not identify a queryable column.")]
    public void OrderingClause_WithNonDBField ()
    {
      IQueryable<Student> query = OrderByTestQueryGenerator.CreateOrderByNonDBPropertyQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();
      Ordering ordering = orderBy.OrderingList.First ();

      OrderingFieldParser parser = new OrderingFieldParser (StubDatabaseInfo.Instance);
      ParseContext parseContext = new ParseContext (
          parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      parser.Parse (ordering.Expression.Body, parseContext, ordering.OrderingDirection);
    }

    [Test]
    public void JoinOrderingClause ()
    {
      IQueryable<Student_Detail> query = JoinTestQueryGenerator.CreateSimpleImplicitOrderByJoin (ExpressionHelper.CreateQuerySource_Detail ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();
      Ordering ordering = orderBy.OrderingList.First ();

      FromClauseBase fromClause = parsedQuery.MainFromClause;
      PropertyInfo relationMember = typeof (Student_Detail).GetProperty ("Student");
      IColumnSource sourceTable = fromClause.GetFromSource (StubDatabaseInfo.Instance); // Student_Detail
      Table relatedTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, relationMember); // Student
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, relationMember);
      
      PropertyInfo orderingMember = typeof (Student).GetProperty ("First");
      SingleJoin join = new SingleJoin (new Column (sourceTable, columns.A), new Column (relatedTable, columns.B));
      FieldSourcePath path = new FieldSourcePath (sourceTable, new[] { join });
      Column? column = DatabaseInfoUtility.GetColumn (StubDatabaseInfo.Instance, relatedTable, orderingMember);
      FieldDescriptor fieldDescriptor = new FieldDescriptor (orderingMember, path, column);

      OrderingFieldParser parser = new OrderingFieldParser (StubDatabaseInfo.Instance);
      ParseContext parseContext = new ParseContext (
          parsedQuery, parsedQuery.GetExpressionTree(), new List<FieldDescriptor>(), _joinedTableContext);
      Assert.AreEqual (new OrderingField (fieldDescriptor, OrderingDirection.Asc), parser.Parse (ordering.Expression.Body, parseContext, ordering.OrderingDirection));
    }

    [Test]
    public void ParserUsesContext()
    {
      Assert.AreEqual (0, _joinedTableContext.Count);
      JoinOrderingClause();
      Assert.AreEqual (1, _joinedTableContext.Count);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Ordering by 'Remotion.Data.UnitTests.Linq.Student_Detail.Student' "
        + "is not supported because it is a relation member.")]
    public void OrderingOnRelationMemberThrows()
    {
      IQueryable<Student_Detail> query = OrderByTestQueryGenerator.CreateRelationMemberOrderByQuery (ExpressionHelper.CreateQuerySource_Detail ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      OrderByClause orderBy = (OrderByClause) parsedQuery.BodyClauses.First ();
      Ordering ordering = orderBy.OrderingList.First ();

      OrderingFieldParser parser = new OrderingFieldParser (StubDatabaseInfo.Instance);
      ParseContext parseContext = new ParseContext (
          parsedQuery, parsedQuery.GetExpressionTree (), new List<FieldDescriptor> (), _joinedTableContext);
      parser.Parse (ordering.Expression.Body, parseContext, ordering.OrderingDirection);
    }
  }
}