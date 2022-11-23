'
' SPDX-FileCopyrightText: 2022 DB Systel GmbH
'
' SPDX-License-Identifier: Apache-2.0
'
' Licensed under the Apache License, Version 2.0 (the "License")
' You may not use this file except in compliance with the License.
'
' You may obtain a copy of the License at
'
'     http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
'
' Author: Frank Schwab, DB Systel GmbH
'
' Version: 1.0.0
'
' Change history:
'    2020-05-05: V1.0.0: Created.
'

Option Strict On
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TUPWLib

<TestClass()> Public Class PackedUnsignedIntegerTest
   Private Const UNEXPECTED_EXCEPTION As String = "Unexpected exception: "

   Private Const MIN_1_BYTE_INTEGER As Integer = 0
   Private Const MAX_1_BYTE_INTEGER As Integer = 63
   Private Const MIN_2_BYTE_INTEGER As Integer = MAX_1_BYTE_INTEGER + 1
   Private Const MAX_2_BYTE_INTEGER As Integer = 16447
   Private Const MIN_3_BYTE_INTEGER As Integer = MAX_2_BYTE_INTEGER + 1
   Private Const MAX_3_BYTE_INTEGER As Integer = 4210751
   Private Const MIN_4_BYTE_INTEGER As Integer = MAX_3_BYTE_INTEGER + 1
   Private Const MAX_4_BYTE_INTEGER As Integer = 1077952575
   Private Const MIN_OVERFLOW_INTEGER As Integer = MAX_4_BYTE_INTEGER + 1

   Private ReadOnly MIN_1_BYTE_PACKED_INTEGER As Byte() = PackedUnsignedInteger.FromInteger(MIN_1_BYTE_INTEGER)
   Private ReadOnly MAX_1_BYTE_PACKED_INTEGER As Byte() = PackedUnsignedInteger.FromInteger(MAX_1_BYTE_INTEGER)
   Private ReadOnly MIN_2_BYTE_PACKED_INTEGER As Byte() = PackedUnsignedInteger.FromInteger(MIN_2_BYTE_INTEGER)
   Private ReadOnly MAX_2_BYTE_PACKED_INTEGER As Byte() = PackedUnsignedInteger.FromInteger(MAX_2_BYTE_INTEGER)
   Private ReadOnly MIN_3_BYTE_PACKED_INTEGER As Byte() = PackedUnsignedInteger.FromInteger(MIN_3_BYTE_INTEGER)
   Private ReadOnly MAX_3_BYTE_PACKED_INTEGER As Byte() = PackedUnsignedInteger.FromInteger(MAX_3_BYTE_INTEGER)
   Private ReadOnly MIN_4_BYTE_PACKED_INTEGER As Byte() = PackedUnsignedInteger.FromInteger(MIN_4_BYTE_INTEGER)
   Private ReadOnly MAX_4_BYTE_PACKED_INTEGER As Byte() = PackedUnsignedInteger.FromInteger(MAX_4_BYTE_INTEGER)

   <TestMethod()> Public Sub TestConversions()
      Assert.AreEqual(1, MIN_1_BYTE_PACKED_INTEGER.Length, "Wrong length of MIN_1_BYTE_PACKED_INTEGER")
      Assert.AreEqual(1, MAX_1_BYTE_PACKED_INTEGER.Length, "Wrong length of MAX_1_BYTE_PACKED_INTEGER")
      Assert.AreEqual(2, MIN_2_BYTE_PACKED_INTEGER.Length, "Wrong length of MIN_2_BYTE_PACKED_INTEGER")
      Assert.AreEqual(2, MAX_2_BYTE_PACKED_INTEGER.Length, "Wrong length of MAX_2_BYTE_PACKED_INTEGER")
      Assert.AreEqual(3, MIN_3_BYTE_PACKED_INTEGER.Length, "Wrong length of MIN_3_BYTE_PACKED_INTEGER")
      Assert.AreEqual(3, MAX_3_BYTE_PACKED_INTEGER.Length, "Wrong length of MAX_3_BYTE_PACKED_INTEGER")
      Assert.AreEqual(4, MIN_4_BYTE_PACKED_INTEGER.Length, "Wrong length of MIN_4_BYTE_PACKED_INTEGER")
      Assert.AreEqual(4, MAX_4_BYTE_PACKED_INTEGER.Length, "Wrong length of MAX_4_BYTE_PACKED_INTEGER")

      Dim test As Integer = PackedUnsignedInteger.ToInteger(MIN_1_BYTE_PACKED_INTEGER)
      Assert.AreEqual(MIN_1_BYTE_INTEGER, test, "MIN_1_BYTE_PACKED_INTEGER is not correctly converted to an integer")

      test = PackedUnsignedInteger.ToInteger(MAX_1_BYTE_PACKED_INTEGER)
      Assert.AreEqual(MAX_1_BYTE_INTEGER, test, "MAX_1_BYTE_PACKED_INTEGER is not correctly converted to an integer")

      test = PackedUnsignedInteger.ToInteger(MIN_2_BYTE_PACKED_INTEGER)
      Assert.AreEqual(MIN_2_BYTE_INTEGER, test, "MIN_2_BYTE_PACKED_INTEGER is not correctly converted to an integer")

      test = PackedUnsignedInteger.ToInteger(MAX_2_BYTE_PACKED_INTEGER)
      Assert.AreEqual(MAX_2_BYTE_INTEGER, test, "MAX_2_BYTE_PACKED_INTEGER is not correctly converted to an integer")

      test = PackedUnsignedInteger.ToInteger(MIN_3_BYTE_PACKED_INTEGER)
      Assert.AreEqual(MIN_3_BYTE_INTEGER, test, "MIN_3_BYTE_PACKED_INTEGER is not correctly converted to an integer")

      test = PackedUnsignedInteger.ToInteger(MAX_3_BYTE_PACKED_INTEGER)
      Assert.AreEqual(MAX_3_BYTE_INTEGER, test, "MAX_3_BYTE_PACKED_INTEGER is not correctly converted to an integer")

      test = PackedUnsignedInteger.ToInteger(MIN_4_BYTE_PACKED_INTEGER)
      Assert.AreEqual(MIN_4_BYTE_INTEGER, test, "MIN_4_BYTE_PACKED_INTEGER is not correctly converted to an integer")

      test = PackedUnsignedInteger.ToInteger(MAX_4_BYTE_PACKED_INTEGER)
      Assert.AreEqual(MAX_4_BYTE_INTEGER, test, "MAX_4_BYTE_PACKED_INTEGER is not correctly converted to an integer")
   End Sub

   <TestMethod()> Public Sub TestExceptions()
      Try
#Disable Warning S1481 ' Unused local variables should be removed
         Dim junk As Byte() = PackedUnsignedInteger.FromInteger(-1)
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail("Exception not thrown on fromInteger = -1")

      Catch ex As Exception
         Assert.IsTrue(ex.Message().Contains("Integer must not be negative"), UNEXPECTED_EXCEPTION & ex.Message())
      End Try

      Try
#Disable Warning S1481 ' Unused local variables should be removed
         Dim junk = PackedUnsignedInteger.FromInteger(MIN_OVERFLOW_INTEGER)
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail("Exception not thrown on fromInteger = " & MIN_OVERFLOW_INTEGER.ToString())

      Catch ex As Exception
         Assert.IsTrue(ex.Message().Contains("Integer too large for packed integer"), UNEXPECTED_EXCEPTION & ex.Message())
      End Try
   End Sub

   <TestMethod()> Public Sub TestToString()
      Assert.AreEqual(MIN_1_BYTE_INTEGER.ToString(),
                      PackedUnsignedInteger.ToString(MIN_1_BYTE_PACKED_INTEGER),
                      "String representation of MIN_1_BYTE_PACKED_INTEGER is not correct")

      Assert.AreEqual(MAX_4_BYTE_INTEGER.ToString(),
                      PackedUnsignedInteger.ToString(MAX_4_BYTE_PACKED_INTEGER),
                      "String representation of MAX_4_BYTE_PACKED_INTEGER is not correct")
   End Sub

End Class