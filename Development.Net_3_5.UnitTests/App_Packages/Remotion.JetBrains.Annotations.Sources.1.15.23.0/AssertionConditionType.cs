// NOTE: This file was originally generated via JetBrains ReSharper
// and is part of the JetBrains.Annoations for ReSharper. 
// It has since been modified for use in the re-motion framework development.
//
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
// Orignal license header by JetBrains:
// 
// Copyright 2007-2012 JetBrains s.r.o.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

#pragma warning disable 1591
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace

namespace JetBrains.Annotations
{
  /// <summary>
  /// Specifies assertion type. If the assertion method argument satisifes the condition, then the execution continues. 
  /// Otherwise, execution is assumed to be halted
  /// </summary>
  enum AssertionConditionType
  {
    /// <summary>
    /// Indicates that the marked parameter should be evaluated to true
    /// </summary>
    IS_TRUE = 0,

    /// <summary>
    /// Indicates that the marked parameter should be evaluated to false
    /// </summary>
    IS_FALSE = 1,

    /// <summary>
    /// Indicates that the marked parameter should be evaluated to null value
    /// </summary>
    IS_NULL = 2,

    /// <summary>
    /// Indicates that the marked parameter should be evaluated to not null value
    /// </summary>
    IS_NOT_NULL = 3,
  }
}