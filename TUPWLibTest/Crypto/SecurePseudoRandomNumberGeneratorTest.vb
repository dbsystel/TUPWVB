'
' SPDX-FileCopyrightText: 2020 DB Systel GmbH
'
' SPDX-License-Identifier: Apache-2.0
'
' Licensed under the Apache License, Version 2.0 (the "License");
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
' Version: 1.0.2
'
' Change history:
'    2020-04-21: V1.0.0: Created.
'    2020-06-19: V1.0.1: Test boundaries where the from value is larger than the span.
'    2020-10-26: V1.0.2: Corrected byte array declaration.
'

'Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports DB.BCM.TUPW

<TestClass()> Public Class SecurePseudoRandomNumberGeneratorTest
   ''' <summary>
   ''' Error message for boundary errors.
   ''' </summary>
   Private Const BOUNDARY_ERROR_MESSAGE As String = "{0} value {1} lies not between {2} and {3}"

   <TestMethod()> Public Sub TestExceptionsForArrayMethods()
      Dim aNullByteArray As Byte()

      Try
#Disable Warning BC42104
         SecurePseudoRandomNumberGenerator.GetBytes(aNullByteArray)
#Enable Warning BC42104

      Catch ex As ArgumentNullException
         '
         ' This exception is expected and so nothing else is done here
         '

      Catch ex As Exception
         Assert.Fail("Expected ArgumentNullException in method 'getBytes' not thrown")
      End Try

      Try
         SecurePseudoRandomNumberGenerator.GetNonZeroBytes(aNullByteArray)

      Catch ex As ArgumentNullException
         '
         ' This exception is expected and so nothing else is done here
         '

      Catch ex As Exception
         Assert.Fail("Expected ArgumentNullException in method 'getNonZeroBytes' not thrown")
      End Try
   End Sub

   <TestMethod()> Public Sub TestSimpleMethods()
      TestSignedByteWithLimits(SByte.MinValue, SByte.MaxValue, False)

      TestByteWithLimits(Byte.MinValue, Byte.MaxValue, False)

      TestShortWithLimits(Short.MinValue, Short.MaxValue, False)

      TestUnsignedShortWithLimits(UShort.MinValue, UShort.MaxValue, False)

      TestIntegerWithLimits(Integer.MinValue, Integer.MaxValue, False)

      TestUnsignedIntegerWithLimits(UInteger.MinValue, UInteger.MaxValue, False)

      TestLongWithLimits(Long.MinValue, Long.MaxValue, False)

      TestUnsignedLongWithLimits(ULong.MinValue, ULong.MaxValue, False)
   End Sub

   <TestMethod()> Public Sub TestSimpleMethodsWithArbitraryBoundaries()
      TestSignedByteWithLimits(-128, 111)

      TestByteWithLimits(17, 111)

      TestShortWithLimits(-65S, 32000S)

      TestUnsignedShortWithLimits(111US, 55555US)

      TestIntegerWithLimits(-1234556, 111111111)

      TestUnsignedIntegerWithLimits(17, 22222222)

      TestLongWithLimits(-888888888L, 9999999999L)

      TestUnsignedLongWithLimits(4444444UL, 87878787878787878UL)
   End Sub

   <TestMethod()> Public Sub TestSimpleMethodsWithSmallSpanArbitraryBoundaries()
      TestSignedByteWithLimits(65, 80)

      TestByteWithLimits(98, 111)

      TestShortWithLimits(32111S, 32222S)

      TestUnsignedShortWithLimits(11111US, 11119US)

      TestIntegerWithLimits(1234556, 1234565)

      TestUnsignedIntegerWithLimits(20000002, 20000022)

      TestLongWithLimits(888888888L, 9999999999L)

      TestUnsignedLongWithLimits(87878787878787800UL, 87878787878787878UL)
   End Sub

   <TestMethod()> Public Sub TestSimpleMethodsWithNearlyMaxBoundaries()
      TestSignedByteWithLimits(SByte.MinValue, SByte.MaxValue - 1)

      TestByteWithLimits(Byte.MinValue, Byte.MaxValue - 1)

      TestShortWithLimits(Short.MinValue, Short.MaxValue - 1S)

      TestUnsignedShortWithLimits(UShort.MinValue, UShort.MaxValue - 1US)

      TestIntegerWithLimits(Integer.MinValue, Integer.MaxValue - 1I)

      TestUnsignedIntegerWithLimits(UInteger.MinValue, UInteger.MaxValue - 1UI)

      TestLongWithLimits(Long.MinValue, Long.MaxValue - 1L)

      TestUnsignedLongWithLimits(ULong.MinValue, ULong.MaxValue - 1UL)
   End Sub

   <TestMethod()> Public Sub TestSimpleMethodsWithNearlyMinBoundaries()
      TestSignedByteWithLimits(SByte.MinValue + 1, SByte.MaxValue)

      TestByteWithLimits(Byte.MinValue + 1, Byte.MaxValue)

      TestShortWithLimits(Short.MinValue + 1S, Short.MaxValue)

      TestUnsignedShortWithLimits(UShort.MinValue + 1US, UShort.MaxValue)

      TestIntegerWithLimits(Integer.MinValue + 1I, Integer.MaxValue)

      TestUnsignedIntegerWithLimits(UInteger.MinValue + 1UI, UInteger.MaxValue)

      TestLongWithLimits(Long.MinValue + 1L, Long.MaxValue)

      TestUnsignedLongWithLimits(ULong.MinValue + 1UL, ULong.MaxValue)
   End Sub

   <TestMethod()> Public Sub TestSimpleMethodsWithMaxBoundaries()
      TestSignedByteWithLimits(SByte.MinValue, SByte.MaxValue)

      TestByteWithLimits(Byte.MinValue, Byte.MaxValue)

      TestShortWithLimits(Short.MinValue, Short.MaxValue)

      TestUnsignedShortWithLimits(UShort.MinValue, UShort.MaxValue)

      TestIntegerWithLimits(Integer.MinValue, Integer.MaxValue)

      TestUnsignedIntegerWithLimits(UInteger.MinValue, UInteger.MaxValue)

      TestLongWithLimits(Long.MinValue, Long.MaxValue)

      TestUnsignedLongWithLimits(ULong.MinValue, ULong.MaxValue)
   End Sub

   <TestMethod()> Public Sub TestByteArrays()
      Dim aByteArray As Byte() = New Byte(0 To 9999) {}  ' This stupid "we want the maximum index and not the size" semantic...

      '
      ' 1. Test that returned bytes cover all bits
      '
      SecurePseudoRandomNumberGenerator.GetBytes(aByteArray)

      Dim collector As Byte = 0

      For Each aByte In aByteArray
         ' This is definitely *not* a silly bit operation as it is performed in a loop
         collector = collector Or aByte
      Next

      Const EXPECTED_COLLECTOR As Byte = &HFF

      Assert.AreEqual(EXPECTED_COLLECTOR, collector, "Overlaying 10,000 random bytes does not cover all bits of a byte. Very strange.")

      '
      ' 2. Test that returned non-zero bytes do not contain a zero and cover all bits
      '
      SecurePseudoRandomNumberGenerator.GetNonZeroBytes(aByteArray)

      Const ZERO_BYTE As Byte = 0

      For Each aByte In aByteArray
         collector = collector Or aByte
         Assert.AreNotEqual(ZERO_BYTE, aByte, "'getNonZeroBytes returned a zero byte")
      Next

      Assert.AreEqual(EXPECTED_COLLECTOR, collector, "Overlaying 10,000 random non-zero bytes does not cover all bits of a byte. Very strange.")
   End Sub


   ' ==============
   ' Helper methods
   ' ==============

   Private Sub TestSignedByteWithLimits(ByVal fromValue As SByte, ByVal toValue As SByte, Optional ByVal randomLimits As Boolean = True)
      Dim aValue As SByte

      If randomLimits Then
         aValue = SecurePseudoRandomNumberGenerator.GetSignedByte(fromValue, toValue)
      Else
         aValue = SecurePseudoRandomNumberGenerator.GetSignedByte()
      End If

      If aValue = 0S Then _
         Console.WriteLine("Getting a signed byte from 'SecurePseudoRandomNumberGenerator' returned 0. This may be correct but not all methods should return 0.")

      TestSignedNumberBoundaries("SByte", aValue, fromValue, toValue)
   End Sub

   Private Sub TestByteWithLimits(ByVal fromValue As Byte, ByVal toValue As Byte, Optional ByVal randomLimits As Boolean = True)
      Dim aValue As Byte

      If randomLimits Then
         aValue = SecurePseudoRandomNumberGenerator.GetByte(fromValue, toValue)
      Else
         aValue = SecurePseudoRandomNumberGenerator.GetByte()
      End If

      If aValue = 0US Then _
         Console.WriteLine("Getting a byte from 'SecurePseudoRandomNumberGenerator' returned 0. This may be correct but not all methods should return 0.")

      TestUnsignedNumberBoundaries("Byte", aValue, fromValue, toValue)
   End Sub

   Private Sub TestShortWithLimits(ByVal fromValue As Short, ByVal toValue As Short, Optional ByVal randomLimits As Boolean = True)
      Dim aValue As Short

      If randomLimits Then
         aValue = SecurePseudoRandomNumberGenerator.GetShort(fromValue, toValue)
      Else
         aValue = SecurePseudoRandomNumberGenerator.GetShort()
      End If

      If aValue = 0S Then _
         Console.WriteLine("Getting a short from 'SecurePseudoRandomNumberGenerator' returned 0. This may be correct but not all methods should return 0.")

      TestSignedNumberBoundaries("Short", aValue, fromValue, toValue)
   End Sub

   Private Sub TestUnsignedShortWithLimits(ByVal fromValue As UShort, ByVal toValue As UShort, Optional ByVal randomLimits As Boolean = True)
      Dim aValue As UShort

      If randomLimits Then
         aValue = SecurePseudoRandomNumberGenerator.GetUnsignedShort(fromValue, toValue)
      Else
         aValue = SecurePseudoRandomNumberGenerator.GetUnsignedShort()
      End If

      If aValue = 0US Then _
         Console.WriteLine("Getting an unsigned short from 'SecurePseudoRandomNumberGenerator' returned 0. This may be correct but not all methods should return 0.")

      TestUnsignedNumberBoundaries("UShort", aValue, fromValue, toValue)
   End Sub

   Private Sub TestIntegerWithLimits(ByVal fromValue As Integer, ByVal toValue As Integer, Optional ByVal randomLimits As Boolean = True)
      Dim aValue As Integer

      If randomLimits Then
         aValue = SecurePseudoRandomNumberGenerator.GetInteger(fromValue, toValue)
      Else
         aValue = SecurePseudoRandomNumberGenerator.GetInteger()
      End If

      If aValue = 0I Then _
         Console.WriteLine("Getting an integer from 'SecurePseudoRandomNumberGenerator' returned 0. This may be correct but not all methods should return 0.")

      TestSignedNumberBoundaries("Integer", aValue, fromValue, toValue)
   End Sub

   Private Sub TestUnsignedIntegerWithLimits(ByVal fromValue As UInteger, ByVal toValue As UInteger, Optional ByVal randomLimits As Boolean = True)
      Dim aValue As UInteger

      If randomLimits Then
         aValue = SecurePseudoRandomNumberGenerator.GetUnsignedInteger(fromValue, toValue)
      Else
         aValue = SecurePseudoRandomNumberGenerator.GetUnsignedInteger()
      End If

      If aValue = 0UI Then _
         Console.WriteLine("Getting an unsigned integer from 'SecurePseudoRandomNumberGenerator' returned 0. This may be correct but not all methods should return 0.")

      TestUnsignedNumberBoundaries("UInteger", aValue, fromValue, toValue)
   End Sub

   Private Sub TestLongWithLimits(ByVal fromValue As Long, ByVal toValue As Long, Optional ByVal randomLimits As Boolean = True)
      Dim aValue As Long

      If randomLimits Then
         aValue = SecurePseudoRandomNumberGenerator.GetLong(fromValue, toValue)
      Else
         aValue = SecurePseudoRandomNumberGenerator.GetLong()
      End If

      If aValue = 0L Then _
         Console.WriteLine("Getting a long from 'SecurePseudoRandomNumberGenerator' returned 0. This may be correct but not all methods should return 0.")

      TestSignedNumberBoundaries("Long", aValue, fromValue, toValue)
   End Sub

   Private Sub TestUnsignedLongWithLimits(ByVal fromValue As ULong, ByVal toValue As ULong, Optional ByVal randomLimits As Boolean = True)
      Dim aValue As ULong

      If randomLimits Then
         aValue = SecurePseudoRandomNumberGenerator.GetUnsignedLong(fromValue, toValue)
      Else
         aValue = SecurePseudoRandomNumberGenerator.GetUnsignedLong()
      End If

      If aValue = 0UL Then _
         Console.WriteLine("Getting an unsigned long from 'SecurePseudoRandomNumberGenerator' returned 0. This may be correct but not all methods should return 0.")

      TestUnsignedNumberBoundaries("ULong", aValue, fromValue, toValue)
   End Sub

   Private Shared Sub TestSignedNumberBoundaries(ByRef variableType As String, ByVal aValue As Long, ByVal minValue As Long, ByVal maxValue As Long)
      Assert.IsTrue((aValue >= minValue) And (aValue <= maxValue), BOUNDARY_ERROR_MESSAGE, variableType, aValue, minValue, maxValue)
   End Sub
   Private Shared Sub TestUnsignedNumberBoundaries(ByRef variableType As String, ByVal aValue As ULong, ByVal minValue As ULong, ByVal maxValue As ULong)
      Assert.IsTrue((aValue >= minValue) And (aValue <= maxValue), BOUNDARY_ERROR_MESSAGE, variableType, aValue, minValue, maxValue)
   End Sub
End Class