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
' Version: 1.1.0
'
' Change history:
'    2020-04-21: V1.0.1: Created.
'    2020-05-15: V1.0.1: Made constants consistent with types.
'    2020-05-27: V1.1.0: Refactored function value mapping into own shared class.
'    2020-06-19: V1.2.1: Use new FunctionValueRangeMapperAPI.
'

Imports System.Security.Cryptography

''' <summary>
''' Provide access to a secure random number generator with a much larger variety of access methods than the default.
''' </summary>
Public NotInheritable Class SecurePseudoRandomNumberGenerator
#Region "Private constants"
   '
   ' It is not possible to write a byte constant in VB.
   '
   Private Const ONE_AS_BYTE As Byte = 1US
   Private Const ONE_AS_SBYTE As SByte = 1S
#End Region

#Region "Class variables"
   ''' <summary>
   ''' Instance of the secure random number generator.
   ''' </summary>
   ''' <remarks>
   ''' <para>The class <see cref="RNGCryptoServiceProvider"/> has a <c>Dispose</c> method. But this is a shared (static) class and so 
   ''' there is no finalization until the assmbly is unloaded. I.e., we do not need to call the <c>Dispose</c> method of
   ''' <see cref="RNGCryptoServiceProvider"/>.</para>
   ''' 
   ''' <para><see cref="RNGCryptoServiceProvider"/> is thread-safe, so there is no need to synchronize access.</para>
   ''' </remarks>
   Private Shared ReadOnly m_Prng As New RNGCryptoServiceProvider()
#End Region

#Region "Public methods"
#Region "Array methods"
   ''' <summary>
   ''' Get pseudo-random bytes into a destination array.
   ''' </summary>
   ''' <param name="destinationArray">Array where the pseudo-random bytes are placed.</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="destinationArray"/> is <c>Nothing</c>.</exception>
   Public Shared Sub GetBytes(destinationArray As Byte())
      RequireNonNull(destinationArray, NameOf(destinationArray))

      m_Prng.GetBytes(destinationArray)
   End Sub

   ''' <summary>
   ''' Get pseudo-random bytes into a destination array with offset and count.
   ''' </summary>
   ''' <param name="destinationArray">Array where the pseudo-random bytes are placed.</param>
   ''' <param name="offset">Offset in the destination array where the pseudo-random bytes are placed.</param>
   ''' <param name="count">Number of bytes that should be placed in the destination array.</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="destinationArray"/> is <c>Nothing</c>.</exception>
   Public Shared Sub GetBytes(destinationArray As Byte(), offset As Integer, count As Integer)
      RequireNonNull(destinationArray, NameOf(destinationArray))

      m_Prng.GetBytes(destinationArray, offset, count)
   End Sub

   ''' <summary>
   ''' Get non-zero pseudo-random bytes into a destination array
   ''' </summary>
   ''' <param name="destinationArray">Array where the non-zero pseudo-random bytes are placed.</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="destinationArray"/> is <c>Nothing</c>.</exception>
   Public Shared Sub GetNonZeroBytes(destinationArray As Byte())
      RequireNonNull(destinationArray, NameOf(destinationArray))

      m_Prng.GetNonZeroBytes(destinationArray)
   End Sub
#End Region

#Region "Basic get methods"
   ''' <summary>
   ''' Get a pseudo-random signed byte.
   ''' </summary>
   ''' <returns>A pseudo-random signed byte.</returns>
   Public Shared Function GetSignedByte() As SByte
      Dim result(0 To 0) As Byte

      m_Prng.GetBytes(result)

#Disable Warning IDE0004
      Return CSByte(CShort(result(0)) + CShort(SByte.MinValue))
#Enable Warning IDE0004
   End Function

   ''' <summary>
   ''' Get a pseudo-random byte.
   ''' </summary>
   ''' <returns>A pseudo-random byte.</returns>
   Public Shared Function GetByte() As Byte
      Dim result(0 To 0) As Byte

      m_Prng.GetBytes(result)

      Return result(0)
   End Function

   ''' <summary>
   ''' Get a pseudo-random short.
   ''' </summary>
   ''' <returns>A pseudo-random short.</returns>
   Public Shared Function GetShort() As Short
      Dim result(0 To 1) As Byte

      m_Prng.GetBytes(result)

      Return BitConverter.ToInt16(result, 0)
   End Function

   ''' <summary>
   ''' Get a pseudo-random integer.
   ''' </summary>
   ''' <returns>A pseudo-random integer.</returns>
   Public Shared Function GetInteger() As Integer
      Dim result(0 To 3) As Byte

      m_Prng.GetBytes(result)

      Return BitConverter.ToInt32(result, 0)
   End Function

   ''' <summary>
   ''' Get a pseudo-random long.
   ''' </summary>
   ''' <returns>A pseudo-random long.</returns>
   Public Shared Function GetLong() As Long
      Dim result(0 To 7) As Byte

      m_Prng.GetBytes(result)

      Return BitConverter.ToInt64(result, 0)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned short.
   ''' </summary>
   ''' <returns>A pseudo-random unsigned short.</returns>
   Public Shared Function GetUnsignedShort() As UShort
      Dim result(0 To 1) As Byte

      m_Prng.GetBytes(result)

      Return BitConverter.ToUInt16(result, 0)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned integer.
   ''' </summary>
   ''' <returns>A pseudo-random unsigned integer.</returns>
   Public Shared Function GetUnsignedInteger() As UInteger
      Dim result(0 To 3) As Byte

      m_Prng.GetBytes(result)

      Return BitConverter.ToUInt32(result, 0)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned long.
   ''' </summary>
   ''' <returns>A pseudo-random unsigned long.</returns>
   Public Shared Function GetUnsignedLong() As ULong
      Dim result(0 To 7) As Byte

      m_Prng.GetBytes(result)

      Return BitConverter.ToUInt64(result, 0)
   End Function
#End Region

#Region "Inclusive range get methods"
   ''' <summary>
   ''' Get a pseudo-random signed byte in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random signed byte that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Shared Function GetSignedByte(fromInclusive As SByte, toInclusive As SByte) As SByte
      Return FunctionValueRangeMapper.GetSignedByte(fromInclusive, toInclusive, AddressOf GetByte)
   End Function

   ''' <summary>
   ''' Get a pseudo-random byte in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random byte that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Shared Function GetByte(fromInclusive As Byte, toInclusive As Byte) As Byte
      Return FunctionValueRangeMapper.GetByte(fromInclusive, toInclusive, AddressOf GetByte)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned short in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random unsigned short that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Shared Function GetUnsignedShort(fromInclusive As UShort, toInclusive As UShort) As UShort
      Return FunctionValueRangeMapper.GetUnsignedShort(fromInclusive, toInclusive, AddressOf GetUnsignedShort)
   End Function

   ''' <summary>
   ''' Get a pseudo-random short in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random short that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Shared Function GetShort(fromInclusive As Short, toInclusive As Short) As Short
      Return FunctionValueRangeMapper.GetShort(fromInclusive, toInclusive, AddressOf GetUnsignedShort)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned integer in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random unsigned integer that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Shared Function GetUnsignedInteger(fromInclusive As UInteger, toInclusive As UInteger) As UInteger
      Return FunctionValueRangeMapper.GetUnsignedInteger(fromInclusive, toInclusive, AddressOf GetUnsignedInteger)
   End Function

   ''' <summary>
   ''' Get a pseudo-random integer in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random integer that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Shared Function GetInteger(fromInclusive As Integer, toInclusive As Integer) As Integer
      Return FunctionValueRangeMapper.GetInteger(fromInclusive, toInclusive, AddressOf GetUnsignedInteger)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned long in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random unsigned long that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Shared Function GetUnsignedLong(fromInclusive As ULong, toInclusive As ULong) As ULong
      Return FunctionValueRangeMapper.GetUnsignedLong(fromInclusive, toInclusive, AddressOf GetUnsignedLong)
   End Function

   ''' <summary>
   ''' Get a pseudo-random long in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <returns>Pseudo-random long that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   Public Shared Function GetLong(fromInclusive As Long, toInclusive As Long) As Long
      Return FunctionValueRangeMapper.GetLong(fromInclusive, toInclusive, AddressOf GetUnsignedLong)
   End Function
#End Region

#Region "Exclusive upper value get methods"
   ''' <summary>
   ''' Get a pseudo-random signed byte in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random signed byte that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="toExclusive"/> is less than 0.</exception>
   Public Shared Function GetSignedByte(toExclusive As SByte) As SByte
      Return GetSignedByte(0, toExclusive - ONE_AS_SBYTE)
   End Function

   ''' <summary>
   ''' Get a pseudo-random byte in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random byte that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   Public Shared Function GetByte(toExclusive As Byte) As Byte
      Return GetByte(0, toExclusive - ONE_AS_BYTE)
   End Function

   ''' <summary>
   ''' Get a pseudo-random short in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random short that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="toExclusive"/> is less than 0.</exception>
   Public Shared Function GetShort(toExclusive As Short) As Short
      Return GetShort(0, toExclusive - 1S)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned short in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random unsigned short that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   Public Shared Function GetUnsignedShort(toExclusive As UShort) As UShort
      Return GetUnsignedShort(0, toExclusive - 1US)
   End Function

   ''' <summary>
   ''' Get a pseudo-random integer in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random integer that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="toExclusive"/> is less than 0.</exception>
   Public Shared Function GetInteger(toExclusive As Integer) As Integer
      Return GetInteger(0, toExclusive - 1I)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned integer in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random unsigned integer that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   Public Shared Function GetUnsignedInteger(toExclusive As UInteger) As UInteger
      Return GetUnsignedInteger(0, toExclusive - 1UI)
   End Function

   ''' <summary>
   ''' Get a pseudo-random long in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random long that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="toExclusive"/> is less than 0.</exception>
   Public Shared Function GetLong(toExclusive As Long) As Long
      Return GetLong(0, toExclusive - 1L)
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned long in a range from 0 to an upper limit.
   ''' </summary>
   ''' <param name="toExclusive">Exclusive end point of the range.</param>
   ''' <returns>Pseudo-random unsigned long that has a value between 0 and <paramref name="toExclusive"/> - 1.</returns>
   Public Shared Function GetUnsignedLong(toExclusive As ULong) As ULong
      Return GetUnsignedLong(0, toExclusive - 1UL)
   End Function
#End Region
#End Region

#Region "Private methods"
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
