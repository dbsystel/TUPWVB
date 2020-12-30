'
' SPDX-FileCopyrightText: 2020 DB Systel GmbH
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
' Version: 1.2.0
'
' Change history:
'    2020-05-05: V1.0.0: Created.
'    2020-05-12: V1.1.0: Corrected relative entropy calculation.
'    2020-05-14: V1.2.0: Exposed no. of processed bytes as a read-only property.
'

''' <summary>
''' Calculate the entropy of byte arrays.
''' </summary>
Public Class EntropyCalculator

#Region "Private constants"
   '******************************************************************
   ' Private constants
   '******************************************************************

   '
   ' Constant for computations
   '
   ''' <summary>
   ''' Log(2) for conversion to logarithm to base 2.
   ''' </summary>
   Private Shared ReadOnly LOG_2 As Double = Math.Log(2)
#End Region

#Region "Instance variables"
   '******************************************************************
   ' Instance variables
   '******************************************************************
   Private ReadOnly m_Counter As UInteger() = New UInteger(0 To 255) {}   ' Array of how many times a specific byte value was counted
   Private m_ByteCount As UInteger = 0                                    ' Number of bytes that have been added to the statistic
#End Region

#Region "Public attributes"
   ''' <summary>
   ''' Get the number of bytes that have been processed.
   ''' </summary>
   ''' <returns>Number of bytes that have been processed.</returns>
   Public ReadOnly Property Count As UInteger
      Get
         Count = m_ByteCount
      End Get
   End Property
#End Region

#Region "Public methods"
   '******************************************************************
   ' Public methods
   '******************************************************************

   ''' <summary>
   ''' Reset the entropy statistics.
   ''' </summary>
   Public Sub Reset()
      ArrayHelper.Clear(m_Counter)

      m_ByteCount = 0
   End Sub

   ''' <summary>
   ''' Add bytes of a byte array to the entropy calculation starting from a specified index and
   ''' for a count of bytes.
   ''' </summary>
   ''' <param name="aByteArray">Byte array to add to the calculation.</param>
   ''' <param name="offset">Start index.</param>
   ''' <param name="count">Number of byte to include.</param>
   Public Sub AddBytes(aByteArray As Byte(), offset As Integer, count As Integer)
      CheckBufferParameters(NameOf(aByteArray), aByteArray, offset, count)

      AddBytesToCounters(aByteArray, offset, count)
   End Sub

   ''' <summary>
   ''' Add bytes of a byte array to entropy calculation starting from a specified index.
   ''' </summary>
   ''' <param name="aByteArray">Byte array to add to the calculation.</param>
   ''' <param name="offset">Start index.</param>
   Public Sub AddBytes(aByteArray As Byte(), offset As Integer)
      RequireNonNull(aByteArray, NameOf(aByteArray))

      AddBytesToCounters(aByteArray, offset, aByteArray.Length - offset)
   End Sub

   ''' <summary>
   ''' Add all bytes of a byte array to the entropy calculation.
   ''' </summary>
   ''' <param name="aByteArray">Byte array to add to the calculation.</param>
   Public Sub AddBytes(aByteArray As Byte())
      RequireNonNull(aByteArray, NameOf(aByteArray))

      AddBytesToCounters(aByteArray, 0, aByteArray.Length)
   End Sub

   ''' <summary>
   ''' Get the entropy per byte.
   ''' </summary>
   ''' <returns>Entropy per byte.</returns>
   Public Function GetEntropy() As Double
      Dim result As Double = 0.0

      If m_ByteCount > 0 Then
         Dim inverseByteCount As Double = 1.0 / m_ByteCount

         Dim p As Double

         For Each value As Integer In m_Counter
            p = value * inverseByteCount

            If p <> 0.0 Then _
               result -= p * Math.Log(p)
         Next
      End If

      Return result / LOG_2
   End Function

   ''' <summary>
   ''' Get the relative entropy-
   ''' </summary>
   ''' <remarks>
   ''' The relative entropy is a value between 0.0 and 1.0 that says how much of the
   ''' maximum possible entropy the actual entropy value is.
   ''' </remarks>
   ''' <returns>Relative entropy.</returns>
   Public Function GetRelativeEntropy() As Double
      Return GetEntropy() * 0.125   ' Maximum entropy is 8, so relative entropy is entropy divided by 8
   End Function

   ''' <summary>
   ''' Gets the information content in bits.
   ''' </summary>
   ''' <remarks>
   ''' Information content is the entropy per byte times the number of bytes.
   ''' </remarks>
   ''' <returns>Information content in bits.</returns>
   Public Function GetInformationInBits() As UInteger
      Dim result As UInteger = 0

      If m_ByteCount > 0 Then _
         result = CUInt(Math.Round(GetEntropy() * m_ByteCount))

      Return result
   End Function
#End Region

#Region "Private methods"
#Region "Check methods"
   ''' <summary>
   ''' Checks the parameters for buffer handling.
   ''' </summary>
   ''' <param name="bufferName">Name of the buffer for use in exception messages.</param>
   ''' <param name="buffer">The buffer to check.</param>
   ''' <param name="offset">The offset into the buffer.</param>
   ''' <param name="count">Count of bytes.</param>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="offset"/> and <paramref name="count"/>
   ''' do not match the <paramref name="buffer"/>.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="buffer"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> or <paramref name="count"/> are
   ''' less than 0 or <paramref name="offset"/> is larger than the length of <paramref name="buffer"/>.</exception>
   Private Shared Sub CheckBufferParameters(bufferName As String, buffer() As Byte, offset As Integer, count As Integer)
      If buffer Is Nothing Then _
         Throw New ArgumentNullException(bufferName)

      If offset < 0 Then _
         Throw New ArgumentOutOfRangeException(NameOf(offset))

      If buffer.Length > 0 AndAlso buffer.Length <= offset Then _
         Throw New ArgumentOutOfRangeException(NameOf(offset))

      If count < 0 Then _
         Throw New ArgumentOutOfRangeException(NameOf(count))

      If buffer.Length - offset < count Then _
         Throw New ArgumentException(bufferName & " is not large enough for data count from offset")
   End Sub
#End Region

#Region "Calculation helpers"
   ''' <summary>
   ''' Add bytes to counter array without any checks.
   ''' </summary>
   ''' <param name="aByteArray">Byte array to add to the calculation.</param>
   ''' <param name="offset">Start index.</param>
   ''' <param name="count">Number of byte to include.</param>
   Private Sub AddBytesToCounters(aByteArray() As Byte, offset As Integer, count As Integer)
      For i As Integer = offset To offset + count - 1
         m_Counter(aByteArray(i)) += 1UI
      Next

      m_ByteCount += CUInt(count)
   End Sub
#End Region


#Region "Exception helpers"
   ''' <summary>
   ''' Check if object is null and throw an exception, if it is.
   ''' </summary>
   ''' <param name="anObject">Object to check.</param>
   ''' <param name="parameterName">Parameter name for exception.</param>
   ''' <exception cref="ArgumentNullException">Thrown when <paramref name="anObject"/> is <c>Nothing</c>.</exception>
   Private Shared Sub RequireNonNull(anObject As Object, parameterName As String)
      If anObject Is Nothing Then _
         Throw New ArgumentNullException(parameterName)
   End Sub
#End Region
#End Region
End Class
