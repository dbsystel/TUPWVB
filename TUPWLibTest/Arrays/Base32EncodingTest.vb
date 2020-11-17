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
' Version: 1.0.0
'
' Change history:
'    2020-04-29: V1.0.0: Created.
'

' Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports DB.BCM.TUPW

<TestClass()> Public Class Base32TestUnit
   Private Shared ReadOnly LOCAL_PRNG As New Random

#Region "Padded encoding tests"
   <TestMethod()> Public Sub TestRandomPaddedEncodeDecode()
      Dim byteArray As Byte()

      For i As Integer = 1 To 50
         Dim arraySize As Integer = LOCAL_PRNG.Next(101)

         ReDim byteArray(0 To arraySize - 1)

         LOCAL_PRNG.NextBytes(byteArray)

         Dim base32Text As String = Base32Encoding.Encode(byteArray)
         Dim decodedArray As Byte() = Base32Encoding.Decode(base32Text)

         CheckByteArray(byteArray, decodedArray)
      Next
   End Sub

   <TestMethod()> Public Sub TestPaddedEncode()
      Dim sourceBytes As Byte() = New Byte() {}

      Dim b32Text As String = Base32Encoding.Encode(sourceBytes)
      Assert.IsTrue(b32Text.Length = 0)

      ReDim sourceBytes(0 To 0)
      sourceBytes(0) = 102
      b32Text = Base32Encoding.Encode(sourceBytes)
      Assert.AreEqual("MY======", b32Text)

      ReDim sourceBytes(0 To 1)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      b32Text = Base32Encoding.Encode(sourceBytes)
      Assert.AreEqual("MZXQ====", b32Text)

      ReDim sourceBytes(0 To 2)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      b32Text = Base32Encoding.Encode(sourceBytes)
      Assert.AreEqual("MZXW6===", b32Text)

      ReDim sourceBytes(0 To 3)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      sourceBytes(3) = 98
      b32Text = Base32Encoding.Encode(sourceBytes)
      Assert.AreEqual("MZXW6YQ=", b32Text)

      ReDim sourceBytes(0 To 4)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      sourceBytes(3) = 98
      sourceBytes(4) = 97
      b32Text = Base32Encoding.Encode(sourceBytes)
      Assert.AreEqual("MZXW6YTB", b32Text)

      ReDim sourceBytes(0 To 5)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      sourceBytes(3) = 98
      sourceBytes(4) = 97
      sourceBytes(5) = 114
      b32Text = Base32Encoding.Encode(sourceBytes)
      Assert.AreEqual("MZXW6YTBOI======", b32Text)
   End Sub

   <TestMethod()> Public Sub TestPaddedInvalidDecodeLengths()
      Dim b32DecodedArray As Byte()

      Dim WRONG_ENCODINGS As String() = {"M=======", "MZX=====", "MZXQ===", "MZXW6Y==", "MZXW6YTBO======="}

      For Each aWrongEncoding As String In WRONG_ENCODINGS
         Try
            b32DecodedArray = Base32Encoding.Decode(aWrongEncoding)

            Assert.Fail($"Expected exception ArgumentNullException not thrown on encoding '{aWrongEncoding}'")

         Catch ex As ArgumentException
            ' Expected exception

         Catch ex As Exception
            Assert.Fail("Unexpected exception: " & ex.Message() & " encoding '" & aWrongEncoding & "'")
         End Try
      Next
   End Sub

   <TestMethod()> Public Sub TestNothingEncode()
      Dim sourceBytes As Byte()

      Try
#Disable Warning S1481 ' Unused local variables should be removed
#Disable Warning BC42104 ' Variable is used before it has been assigned a value
         Dim b32Text As String = Base32Encoding.Encode(sourceBytes)
#Enable Warning BC42104 ' Variable is used before it has been assigned a value
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail("Expected exception ArgumentNullException not thrown")

      Catch ex As ArgumentNullException
         ' This exception is expected

      Catch ex As Exception
         Assert.Fail("Unexpected exception: " & ex.Message())
      End Try
   End Sub
#End Region

#Region "Padded spell-safe encoding tests"
   <TestMethod()> Public Sub TestRandomPaddedSpellSafeEncodeDecode()
      Dim byteArray As Byte()

      For i As Integer = 1 To 50
         Dim arraySize As Integer = LOCAL_PRNG.Next(101)

         ReDim byteArray(0 To arraySize - 1)

         LOCAL_PRNG.NextBytes(byteArray)

         Dim base32Text As String = Base32Encoding.EncodeSpellSafe(byteArray)
         Dim decodedArray As Byte() = Base32Encoding.DecodeSpellSafe(base32Text)

         CheckByteArray(byteArray, decodedArray)
      Next
   End Sub

   <TestMethod()> Public Sub TestPaddedSpellSafeEncode()
      Dim sourceBytes As Byte() = New Byte() {}

      Dim b32Text As String = Base32Encoding.EncodeSpellSafe(sourceBytes)
      Assert.IsTrue(b32Text.Length = 0)

      ReDim sourceBytes(0 To 0)
      sourceBytes(0) = 102
      b32Text = Base32Encoding.EncodeSpellSafe(sourceBytes)
      Assert.AreEqual("Jj======", b32Text)

      ReDim sourceBytes(0 To 1)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      b32Text = Base32Encoding.EncodeSpellSafe(sourceBytes)
      Assert.AreEqual("JkhT====", b32Text)

      ReDim sourceBytes(0 To 2)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      b32Text = Base32Encoding.EncodeSpellSafe(sourceBytes)
      Assert.AreEqual("Jkhgx===", b32Text)

      ReDim sourceBytes(0 To 3)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      sourceBytes(3) = 98
      b32Text = Base32Encoding.EncodeSpellSafe(sourceBytes)
      Assert.AreEqual("JkhgxjT=", b32Text)

      ReDim sourceBytes(0 To 4)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      sourceBytes(3) = 98
      sourceBytes(4) = 97
      b32Text = Base32Encoding.EncodeSpellSafe(sourceBytes)
      Assert.AreEqual("JkhgxjZ3", b32Text)

      ReDim sourceBytes(0 To 5)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      sourceBytes(3) = 98
      sourceBytes(4) = 97
      sourceBytes(5) = 114
      b32Text = Base32Encoding.EncodeSpellSafe(sourceBytes)
      Assert.AreEqual("JkhgxjZ3NC======", b32Text)
   End Sub

#End Region

#Region "Unpadded encoding tests"
   <TestMethod()> Public Sub TestRandomUnpaddedEncodeDecode()
      Dim byteArray As Byte()

      For i As Integer = 1 To 50
         Dim arraySize As Integer = LOCAL_PRNG.Next(101)

         ReDim byteArray(0 To arraySize - 1)

         LOCAL_PRNG.NextBytes(byteArray)

         Dim base32Text As String = Base32Encoding.EncodeNoPadding(byteArray)
         Dim decodedArray As Byte() = Base32Encoding.Decode(base32Text)

         CheckByteArray(byteArray, decodedArray)
      Next
   End Sub

   <TestMethod()> Public Sub TestUnpaddedEncode()
      Dim sourceBytes As Byte() = New Byte() {}

      Dim b32Text As String = Base32Encoding.EncodeNoPadding(sourceBytes)
      Assert.IsTrue(b32Text.Length = 0)

      ReDim sourceBytes(0 To 0)
      sourceBytes(0) = 102
      b32Text = Base32Encoding.EncodeNoPadding(sourceBytes)
      Assert.AreEqual("MY", b32Text)

      ReDim sourceBytes(0 To 1)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      b32Text = Base32Encoding.EncodeNoPadding(sourceBytes)
      Assert.AreEqual("MZXQ", b32Text)

      ReDim sourceBytes(0 To 2)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      b32Text = Base32Encoding.EncodeNoPadding(sourceBytes)
      Assert.AreEqual("MZXW6", b32Text)

      ReDim sourceBytes(0 To 3)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      sourceBytes(3) = 98
      b32Text = Base32Encoding.EncodeNoPadding(sourceBytes)
      Assert.AreEqual("MZXW6YQ", b32Text)

      ReDim sourceBytes(0 To 4)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      sourceBytes(3) = 98
      sourceBytes(4) = 97
      b32Text = Base32Encoding.EncodeNoPadding(sourceBytes)
      Assert.AreEqual("MZXW6YTB", b32Text)

      ReDim sourceBytes(0 To 5)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      sourceBytes(3) = 98
      sourceBytes(4) = 97
      sourceBytes(5) = 114
      b32Text = Base32Encoding.EncodeNoPadding(sourceBytes)
      Assert.AreEqual("MZXW6YTBOI", b32Text)
   End Sub
#End Region

#Region "Unpadded spell-safe encoding tests"
   <TestMethod()> Public Sub TestRandomUnpaddedSpellSafeEncodeDecode()
      Dim byteArray As Byte()

      For i As Integer = 1 To 50
         Dim arraySize As Integer = LOCAL_PRNG.Next(101)

         ReDim byteArray(0 To arraySize - 1)

         LOCAL_PRNG.NextBytes(byteArray)

         Dim base32Text As String = Base32Encoding.EncodeSpellSafeNoPadding(byteArray)
         Dim decodedArray As Byte() = Base32Encoding.DecodeSpellSafe(base32Text)

         CheckByteArray(byteArray, decodedArray)
      Next
   End Sub

   <TestMethod()> Public Sub TestUnpaddedSpellSafeEncode()
      Dim sourceBytes As Byte() = New Byte() {}

      Dim b32Text As String = Base32Encoding.EncodeSpellSafeNoPadding(sourceBytes)
      Assert.IsTrue(b32Text.Length = 0)

      ReDim sourceBytes(0 To 0)
      sourceBytes(0) = 102
      b32Text = Base32Encoding.EncodeSpellSafeNoPadding(sourceBytes)
      Assert.AreEqual("Jj", b32Text)

      ReDim sourceBytes(0 To 1)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      b32Text = Base32Encoding.EncodeSpellSafeNoPadding(sourceBytes)
      Assert.AreEqual("JkhT", b32Text)

      ReDim sourceBytes(0 To 2)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      b32Text = Base32Encoding.EncodeSpellSafeNoPadding(sourceBytes)
      Assert.AreEqual("Jkhgx", b32Text)

      ReDim sourceBytes(0 To 3)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      sourceBytes(3) = 98
      b32Text = Base32Encoding.EncodeSpellSafeNoPadding(sourceBytes)
      Assert.AreEqual("JkhgxjT", b32Text)

      ReDim sourceBytes(0 To 4)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      sourceBytes(3) = 98
      sourceBytes(4) = 97
      b32Text = Base32Encoding.EncodeSpellSafeNoPadding(sourceBytes)
      Assert.AreEqual("JkhgxjZ3", b32Text)

      ReDim sourceBytes(0 To 5)
      sourceBytes(0) = 102
      sourceBytes(1) = 111
      sourceBytes(2) = 111
      sourceBytes(3) = 98
      sourceBytes(4) = 97
      sourceBytes(5) = 114
      b32Text = Base32Encoding.EncodeSpellSafeNoPadding(sourceBytes)
      Assert.AreEqual("JkhgxjZ3NC", b32Text)
   End Sub
#End Region

#Region "Padded decoding tests"
   <TestMethod()> Public Sub TestPaddedDecode()
      Dim b32DecodedArray As Byte()

      b32DecodedArray = Base32Encoding.Decode("")
      Assert.IsTrue(b32DecodedArray.Length = 0)

      Dim expectedResult As Byte()

      b32DecodedArray = Base32Encoding.Decode("MY======")
      ReDim expectedResult(0 To 0)
      expectedResult(0) = 102
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.Decode("MZXQ====")
      ReDim expectedResult(0 To 1)
      expectedResult(0) = 102
      expectedResult(1) = 111
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.Decode("MZXW6===")
      ReDim expectedResult(0 To 2)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.Decode("MZXW6YQ=")
      ReDim expectedResult(0 To 3)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      expectedResult(3) = 98
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.Decode("MZXW6YTB")
      ReDim expectedResult(0 To 4)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      expectedResult(3) = 98
      expectedResult(4) = 97
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.Decode("MZXW6YTBOI======")
      ReDim expectedResult(0 To 5)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      expectedResult(3) = 98
      expectedResult(4) = 97
      expectedResult(5) = 114
      CheckByteArray(b32DecodedArray, expectedResult)
   End Sub

   <TestMethod()> Public Sub TestPaddedInvalidCharacterDecode()
      Dim b32DecodedArray As Byte()

      Try
         b32DecodedArray = Base32Encoding.Decode("M1======")

         Assert.Fail("Expected exception ArgumentException not thrown")

      Catch ex As ArgumentException
         ' Expected exception

      Catch ex As Exception
         Assert.Fail("Unexpected exception: " & ex.Message())
      End Try
   End Sub

   <TestMethod()> Public Sub TestNothingDecode()
      Dim sourceString As String = Nothing

      Try
#Disable Warning S1481 ' Unused local variables should be removed
         Dim b32DecodeArray As Byte() = Base32Encoding.Decode(sourceString)
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail("Expected exception ArgumentNullException not thrown")

      Catch ex As ArgumentNullException
         ' This exception is expected

      Catch ex As Exception
         Assert.Fail("Unexpected exception: " & ex.Message())
      End Try
   End Sub
#End Region

#Region "Padded spell-safe decoding tests"
   <TestMethod()> Public Sub TestPaddedSpellSafeDecode()
      Dim b32DecodedArray As Byte()

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("")
      Assert.IsTrue(b32DecodedArray.Length = 0)

      Dim expectedResult As Byte()

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("Jj======")
      ReDim expectedResult(0 To 0)
      expectedResult(0) = 102
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("JkhT====")
      ReDim expectedResult(0 To 1)
      expectedResult(0) = 102
      expectedResult(1) = 111
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("Jkhgx===")
      ReDim expectedResult(0 To 2)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("JkhgxjT=")
      ReDim expectedResult(0 To 3)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      expectedResult(3) = 98
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("JkhgxjZ3")
      ReDim expectedResult(0 To 4)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      expectedResult(3) = 98
      expectedResult(4) = 97
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("JkhgxjZ3NC======")
      ReDim expectedResult(0 To 5)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      expectedResult(3) = 98
      expectedResult(4) = 97
      expectedResult(5) = 114
      CheckByteArray(b32DecodedArray, expectedResult)
   End Sub

   <TestMethod()> Public Sub TestPaddedSpellSafeInvalidCharacterDecode1()
      Dim b32DecodedArray As Byte()

      Try
         b32DecodedArray = Base32Encoding.DecodeSpellSafe("M1======")

         Assert.Fail("Expected exception ArgumentException not thrown")

      Catch ex As ArgumentException
         ' Expected exception

      Catch ex As Exception
         Assert.Fail("Unexpected exception: " & ex.Message())
      End Try
   End Sub

   <TestMethod()> Public Sub TestPaddedSpellSafeInvalidCharacterDecode2()
      Dim b32DecodedArray As Byte()

      Try
         b32DecodedArray = Base32Encoding.DecodeSpellSafe("Ms======")

         Assert.Fail("Expected exception ArgumentException not thrown")

      Catch ex As ArgumentException
         ' Expected exception

      Catch ex As Exception
         Assert.Fail("Unexpected exception: " & ex.Message())
      End Try
   End Sub

   <TestMethod()> Public Sub TestSpellSafeNothingDecode()
      Dim sourceString As String = Nothing

      Try
#Disable Warning S1481 ' Unused local variables should be removed
         Dim b32DecodeArray As Byte() = Base32Encoding.DecodeSpellSafe(sourceString)
#Enable Warning S1481 ' Unused local variables should be removed

         Assert.Fail("Expected exception ArgumentNullException not thrown")

      Catch ex As ArgumentNullException
         ' This exception is expected

      Catch ex As Exception
         Assert.Fail("Unexpected exception: " & ex.Message())
      End Try
   End Sub
#End Region

#Region "Unpadded decoding tests"
   <TestMethod()> Public Sub TestUnpaddedDecode()
      Dim b32DecodedArray As Byte()

      b32DecodedArray = Base32Encoding.Decode("")
      Assert.IsTrue(b32DecodedArray.Length = 0)

      b32DecodedArray = Base32Encoding.Decode("MY")
      Dim expectedResult(0 To 0) As Byte
      expectedResult(0) = 102
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.Decode("MZXQ")
      ReDim expectedResult(0 To 1)
      expectedResult(0) = 102
      expectedResult(1) = 111
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.Decode("MZXW6")
      ReDim expectedResult(0 To 2)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.Decode("MZXW6YQ")
      ReDim expectedResult(0 To 3)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      expectedResult(3) = 98
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.Decode("MZXW6YTB")
      ReDim expectedResult(0 To 4)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      expectedResult(3) = 98
      expectedResult(4) = 97
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.Decode("MZXW6YTBOI")
      ReDim expectedResult(0 To 5)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      expectedResult(3) = 98
      expectedResult(4) = 97
      expectedResult(5) = 114
      CheckByteArray(b32DecodedArray, expectedResult)
   End Sub

   <TestMethod()> Public Sub TestUnpaddedInvalidDecodeLengths()
      Dim b32DecodedArray As Byte()

      Dim WRONG_ENCODINGS As String() = {"M", "MZX", "MZXW6Y", "MZXW6YTBO"}

      For Each aWrongEncoding As String In WRONG_ENCODINGS
         Try
#Disable Warning S1481 ' Unused local variables should be removed
            b32DecodedArray = Base32Encoding.Decode(aWrongEncoding)
#Enable Warning S1481 ' Unused local variables should be removed

            Assert.Fail($"Expected exception ArgumentNullException not thrown on encoding '${aWrongEncoding}'")

         Catch ex As ArgumentException
            ' Expected exception

         Catch ex As Exception
            Assert.Fail("Unexpected exception: " & ex.Message() & " encoding '" & aWrongEncoding & "'")
         End Try
      Next
   End Sub

   <TestMethod()> Public Sub TestUnpaddedInvalidCharacterDecode()
      Dim b32DecodedArray As Byte()

      Try
         b32DecodedArray = Base32Encoding.Decode("M1")

         Assert.Fail("Expected exception ArgumentException not thrown")

      Catch ex As ArgumentException
         ' Expected exception

      Catch ex As Exception
         Assert.Fail("Unexpected exception: " & ex.Message())
      End Try
   End Sub
#End Region

#Region "Unpadded spell-safe decoding tests"
   <TestMethod()> Public Sub TestUnpaddedSpellSafeDecode()
      Dim b32DecodedArray As Byte()

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("")
      Assert.IsTrue(b32DecodedArray.Length = 0)

      Dim expectedResult As Byte()

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("Jj")
      ReDim expectedResult(0 To 0)
      expectedResult(0) = 102
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("JkhT")
      ReDim expectedResult(0 To 1)
      expectedResult(0) = 102
      expectedResult(1) = 111
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("Jkhgx")
      ReDim expectedResult(0 To 2)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("JkhgxjT")
      ReDim expectedResult(0 To 3)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      expectedResult(3) = 98
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("JkhgxjZ3")
      ReDim expectedResult(0 To 4)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      expectedResult(3) = 98
      expectedResult(4) = 97
      CheckByteArray(b32DecodedArray, expectedResult)

      b32DecodedArray = Base32Encoding.DecodeSpellSafe("JkhgxjZ3NC")
      ReDim expectedResult(0 To 5)
      expectedResult(0) = 102
      expectedResult(1) = 111
      expectedResult(2) = 111
      expectedResult(3) = 98
      expectedResult(4) = 97
      expectedResult(5) = 114
      CheckByteArray(b32DecodedArray, expectedResult)
   End Sub

   <TestMethod()> Public Sub TestUnpaddedSpellSafeInvalidCharacterDecode1()
      Dim b32DecodedArray As Byte()

      Try
         b32DecodedArray = Base32Encoding.DecodeSpellSafe("M1")

         Assert.Fail("Expected exception ArgumentException not thrown")

      Catch ex As ArgumentException
         ' Expected exception

      Catch ex As Exception
         Assert.Fail("Unexpected exception: " & ex.Message())
      End Try
   End Sub

   <TestMethod()> Public Sub TestUnpaddedSpellSafeInvalidCharacterDecode2()
      Dim b32DecodedArray As Byte()

      Try
         b32DecodedArray = Base32Encoding.DecodeSpellSafe("MN")

         Assert.Fail("Expected exception ArgumentException not thrown")

      Catch ex As ArgumentException
         ' Expected exception

      Catch ex As Exception
         Assert.Fail("Unexpected exception: " & ex.Message())
      End Try
   End Sub
#End Region

#Region "Private helper methods"
   Private Sub CheckByteArray(expectedArray As Byte(), resultArray() As Byte)
      Assert.AreEqual(expectedArray.Length, resultArray.Length)

      For i As Integer = 0 To resultArray.Length - 1
         Assert.AreEqual(expectedArray(i), resultArray(i))
      Next
   End Sub
#End Region
End Class