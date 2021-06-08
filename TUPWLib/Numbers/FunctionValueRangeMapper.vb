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
' Version: 2.0.2
'
' Change history:
'    2020-05-27: V1.0.0: Created.
'    2020-06-05: V1.0.1: Changed delegate name according to MS naming conventions.
'    2020-06-19: V2.0.0: Corrected calculation of signed numbers with inclusive boundaries and simplified API.
'    2020-08-13: V2.0.1: Declared delegates explicitely as public.
'    2020-08-13: V2.0.2: Cleaned up byte mask helper.
'

''' <summary>
''' Map function values to ranges.
''' </summary>
Public NotInheritable Class FunctionValueRangeMapper
#Region "Private constants"
   '
   ' It is not possible to write a byte constant in VB.
   '
   Private Const ZERO_AS_BYTE As Byte = 0US
   Private Const ONE_AS_BYTE As Byte = 1US
   Private Const MSB_AS_BYTE As Byte = &H80US
   Private Const ZERO_AS_SBYTE As SByte = 0S

   '
   ' Signed maximum values as unsigned types for signed number calculation.
   '
   Private Const SBYTE_MAX_VALUE_AS_BYTE As Byte = SByte.MaxValue
   Private Const SHORT_MAX_VALUE_AS_USHORT As UShort = Short.MaxValue
   Private Const INTEGER_MAX_VALUE_AS_UINTEGER As UInteger = Integer.MaxValue
   Private Const LONG_MAX_VALUE_AS_ULONG As ULong = Long.MaxValue
#End Region

#Region "Public types"
   Public Delegate Function GetByteBasicCallback() As Byte
   Public Delegate Function GetUnsignedShortBasicCallback() As UShort
   Public Delegate Function GetUnsignedIntegerBasicCallback() As UInteger
   Public Delegate Function GetUnsignedLongBasicCallback() As ULong
#End Region

#Region "Public methods"
   ''' <summary>
   ''' Get a pseudo-random signed byte in a range.
   ''' </summary>
   ''' <remarks>
   ''' This method returns a signed number but the delegate has to be the one of the <em>unsigned</em> type.
   ''' </remarks>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <param name="getByteBasicFunction">Delegate of function that gets one byte.</param>
   ''' <returns>Pseudo-random signed byte that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="getByteBasicFunction"/> is <c>Nothing</c>.</exception>
   Public Shared Function GetSignedByte(fromInclusive As SByte,
                                        toInclusive As SByte,
                                        getByteBasicFunction As GetByteBasicCallback) As SByte
      If getByteBasicFunction Is Nothing Then _
         Throw New ArgumentNullException(NameOf(getByteBasicFunction))

      Dim result As SByte

      If toInclusive >= fromInclusive Then
         '
         ' We need to work with the unsigned data type here as this is the only way to correctly work with the mask method.
         ' Negative numbers can not be used with masks. So we calculate the span of the boundaries, get an unsigned result
         ' in that span and add this result to the "from" boundary.
         '
         Dim unsignedSpan As Byte

         If (fromInclusive < ZERO_AS_SBYTE) AndAlso (toInclusive >= ZERO_AS_SBYTE) Then
            unsignedSpan = CByte(Not fromInclusive) + ONE_AS_BYTE + CByte(toInclusive)   ' Add the negative number in a way that can not overflow
         Else
            unsignedSpan = CByte(toInclusive - fromInclusive)
         End If

         Dim unsignedSpanResult As Byte = GetByte(ZERO_AS_BYTE, unsignedSpan, getByteBasicFunction)

         '
         ' Now we are in signed/unsigned hell: We can not add an unsigned number that is larger than the
         ' maximum value of the signed type. We have to check for this situation.
         '
         If unsignedSpanResult <= SBYTE_MAX_VALUE_AS_BYTE Then
            ' This is the easy part: Just add the number.
            result = fromInclusive + CSByte(unsignedSpanResult)
         Else
            ' If the unsigned number is larger than the maximum value of the signed type we first add the
            ' maximum number and then the rest. This will never overflow.
            result = fromInclusive + SByte.MaxValue
            result += CSByte(unsignedSpanResult - SBYTE_MAX_VALUE_AS_BYTE)
         End If
      Else
         ThrowRangeException()
      End If

      Return result
   End Function

   ''' <summary>
   ''' Get a pseudo-random byte in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <param name="getByteBasicFunction">Delegate of function that gets one byte.</param>
   ''' <returns>Pseudo-random byte that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="getByteBasicFunction"/> is <c>Nothing</c>.</exception>
   Public Shared Function GetByte(fromInclusive As Byte,
                                  toInclusive As Byte,
                                  getByteBasicFunction As GetByteBasicCallback) As Byte
      If getByteBasicFunction Is Nothing Then _
         Throw New ArgumentNullException(NameOf(getByteBasicFunction))

      Dim result As Byte

      If toInclusive >= fromInclusive Then
         If (toInclusive < Byte.MaxValue) OrElse (fromInclusive > Byte.MinValue) Then
            ' Calculate the size of the interval that should be returned
            Dim maxValue As Byte = toInclusive - fromInclusive  ' This is always nonnegative
            Dim size As Byte = maxValue + ONE_AS_BYTE           ' This is always positive

            ' If the size is a power of 2 we are done
            If (size And maxValue) = ZERO_AS_BYTE Then
               result = getByteBasicFunction() And maxValue
            Else
               ' Size is not a power of two, so we need to calculate a pseudo-random
               ' number that is not biased

               ' Calculate the mask for the smallest power of two that is larger than maxValue
               Dim mask As Byte = GetByteMaskForValue(maxValue)

               ' Now get a random number with the mask laid over it and reject all values that are too large
               Do
                  result = getByteBasicFunction() And mask
               Loop While result > maxValue
            End If
         Else
            ' We only get here if 'fromInclusive = MinValue' and 'toInclusive = MaxValue'. I.e. get a number without any boundaries.
            result = getByteBasicFunction()
         End If
      Else
         ThrowRangeException()
      End If

      ' Return the calculated pseudo-random number in the interval plus the minimum value
      Return result + fromInclusive
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned short in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <param name="getUnsignedShortBasicFunction">Delegate of function that gets one unsigned short.</param>
   ''' <returns>Pseudo-random unsigned short that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="getUnsignedShortBasicFunction"/> is <c>Nothing</c>.</exception>
   Public Shared Function GetUnsignedShort(fromInclusive As UShort,
                                           toInclusive As UShort,
                                           getUnsignedShortBasicFunction As GetUnsignedShortBasicCallback) As UShort
      If getUnsignedShortBasicFunction Is Nothing Then _
         Throw New ArgumentNullException(NameOf(getUnsignedShortBasicFunction))

      Dim result As UShort

      If toInclusive >= fromInclusive Then
         If (toInclusive < UShort.MaxValue) OrElse (fromInclusive > UShort.MinValue) Then
            ' Calculate the size of the interval that should be returned
            Dim maxValue As UShort = toInclusive - fromInclusive  ' This is always nonnegative
            Dim size As UShort = maxValue + 1US                   ' This is always positive

            ' If the size is a power of 2 we are done
            If (size And maxValue) = 0US Then
               result = getUnsignedShortBasicFunction() And maxValue
            Else
               ' Size is not a power of two, so we need to calculate a pseudo-random
               ' number that is not biased

               ' Calculate the mask for the smallest power of two that is larger than maxValue
               Dim mask As UShort = GetUnsignedShortMaskForValue(maxValue)

               ' Now get a random number with the mask laid over it and reject all values that are too large
               Do
                  result = getUnsignedShortBasicFunction() And mask
               Loop While result > maxValue
            End If
         Else
            ' We only get here if 'fromInclusive = MinValue' and 'toInclusive = MaxValue'. I.e. get a number without any boundaries.
            result = getUnsignedShortBasicFunction()
         End If
      Else
         ThrowRangeException()
      End If

      ' Return the calculated pseudo-random number in the interval plus the minimum value
      Return result + fromInclusive
   End Function

   ''' <summary>
   ''' Get a pseudo-random short in a range.
   ''' </summary>
   ''' <remarks>
   ''' This method returns a signed number but the delegate has to be the one of the <em>unsigned</em> type.
   ''' </remarks>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <param name="getUnsignedShortBasicFunction">Delegate of function that gets one unsigned short.</param>
   ''' <returns>Pseudo-random short that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="getUnsignedShortBasicFunction"/> is <c>Nothing</c>.</exception>
   Public Shared Function GetShort(fromInclusive As Short,
                                   toInclusive As Short,
                                   getUnsignedShortBasicFunction As GetUnsignedShortBasicCallback) As Short
      If getUnsignedShortBasicFunction Is Nothing Then _
         Throw New ArgumentNullException(NameOf(getUnsignedShortBasicFunction))

      Dim result As Short

      If toInclusive >= fromInclusive Then
         '
         ' We need to work with the unsigned data type here as this is the only way to correctly work with the mask method.
         ' Negative numbers can not be used with masks. So we calculate the span of the boundaries, get an unsigned result
         ' in that span and add this result to the "from" boundary.
         '
         Dim unsignedSpan As UShort

         If (fromInclusive < 0S) AndAlso (toInclusive >= 0S) Then
            unsignedSpan = CUShort(Not fromInclusive) + 1US + CUShort(toInclusive)   ' Add the negative number in a way that can not overflow
         Else
            unsignedSpan = CUShort(toInclusive - fromInclusive)
         End If

         Dim unsignedSpanResult As UShort = GetUnsignedShort(0US, unsignedSpan, getUnsignedShortBasicFunction)

         '
         ' Now we are in signed/unsigned hell: We can not add an unsigned number that is larger than the
         ' maximum value of the signed type. We have to check for this situation.
         '
         If unsignedSpanResult <= SHORT_MAX_VALUE_AS_USHORT Then
            ' This is the easy part: Just add the number.
            result = fromInclusive + CShort(unsignedSpanResult)
         Else
            ' If the unsigned number is larger than the maximum value of the signed type we first add the
            ' maximum number and then the rest. This will never overflow.
            result = fromInclusive + Short.MaxValue
            result += CShort(unsignedSpanResult - SHORT_MAX_VALUE_AS_USHORT)
         End If
      Else
         ThrowRangeException()
      End If

      Return result
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned integer in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <param name="getUnsignedIntegerBasicFunction">Delegate of function that gets one unsigned integer.</param>
   ''' <returns>Pseudo-random unsigned integer that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="getUnsignedIntegerBasicFunction"/> is <c>Nothing</c>.</exception>
   Public Shared Function GetUnsignedInteger(fromInclusive As UInteger,
                                             toInclusive As UInteger,
                                             getUnsignedIntegerBasicFunction As GetUnsignedIntegerBasicCallback) As UInteger
      If getUnsignedIntegerBasicFunction Is Nothing Then _
         Throw New ArgumentNullException(NameOf(getUnsignedIntegerBasicFunction))

      Dim result As UInteger

      If toInclusive >= fromInclusive Then
         If (toInclusive < UInteger.MaxValue) OrElse (fromInclusive > UInteger.MinValue) Then
            ' Calculate the size of the interval that should be returned
            Dim maxValue As UInteger = toInclusive - fromInclusive  ' This is always nonnegative
            Dim size As UInteger = maxValue + 1UI                   ' This is always positive

            ' If the size is a power of 2 we are done
            If (size And maxValue) = 0UI Then
               result = getUnsignedIntegerBasicFunction() And maxValue
            Else
               ' Size is not a power of two, so we need to calculate a pseudo-random
               ' number that is not biased

               ' Calculate the mask for the smallest power of two that is larger than maxValue
               Dim mask As UInteger = GetUnsignedIntegerMaskForValue(maxValue)

               ' Now get a random number with the mask laid over it and reject all values that are too large
               Do
                  result = getUnsignedIntegerBasicFunction() And mask
               Loop While result > maxValue
            End If
         Else
            ' We only get here if 'fromInclusive = MinValue' and 'toInclusive = MaxValue'. I.e. get a number without any boundaries.
            result = getUnsignedIntegerBasicFunction()
         End If
      Else
         ThrowRangeException()
      End If

      ' Return the calculated pseudo-random number in the interval plus the minimum value
      Return result + fromInclusive
   End Function

   ''' <summary>
   ''' Get a pseudo-random integer in a range.
   ''' </summary>
   ''' <remarks>
   ''' This method returns a signed number but the delegate has to be the one of the <em>unsigned</em> type.
   ''' </remarks>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <param name="getUnsignedIntegerBasicFunction">Delegate of function that gets one unsigned integer.</param>
   ''' <returns>Pseudo-random integer that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="getUnsignedIntegerBasicFunction"/> is <c>Nothing</c>.</exception>
   Public Shared Function GetInteger(fromInclusive As Integer,
                                     toInclusive As Integer,
                                     getUnsignedIntegerBasicFunction As GetUnsignedIntegerBasicCallback) As Integer
      If getUnsignedIntegerBasicFunction Is Nothing Then _
         Throw New ArgumentNullException(NameOf(getUnsignedIntegerBasicFunction))

      Dim result As Integer

      If toInclusive >= fromInclusive Then
         '
         ' We need to work with the unsigned data type here as this is the only way to correctly work with the mask method.
         ' Negative numbers can not be used with masks. So we calculate the span of the boundaries, get an unsigned result
         ' in that span and add this result to the "from" boundary.
         '
         Dim unsignedSpan As UInteger

         If (fromInclusive < 0I) AndAlso (toInclusive >= 0I) Then
            unsignedSpan = CUInt(Not fromInclusive) + 1UI + CUInt(toInclusive)   ' Add the negative number in a way that can not overflow
         Else
            unsignedSpan = CUInt(toInclusive - fromInclusive)
         End If

         Dim unsignedSpanResult As UInteger = GetUnsignedInteger(0UI, unsignedSpan, getUnsignedIntegerBasicFunction)

         '
         ' Now we are in signed/unsigned hell: We can not add an unsigned number that is larger than the
         ' maximum value of the signed type. We have to check for this situation.
         '
         If unsignedSpanResult <= INTEGER_MAX_VALUE_AS_UINTEGER Then
            ' This is the easy part: Just add the number.
            result = fromInclusive + CInt(unsignedSpanResult)
         Else
            ' If the unsigned number is larger than the maximum value of the signed type we first add the
            ' maximum number and then the rest. This will never overflow.
            result = fromInclusive + Integer.MaxValue
            result += CInt(unsignedSpanResult - INTEGER_MAX_VALUE_AS_UINTEGER)
         End If
      Else
         ThrowRangeException()
      End If

      Return result
   End Function

   ''' <summary>
   ''' Get a pseudo-random unsigned long in a range.
   ''' </summary>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <param name="getUnsignedLongBasicFunction">Delegate of function that gets one unsigned long.</param>
   ''' <returns>Pseudo-random unsigned long that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="getUnsignedLongBasicFunction"/> is <c>Nothing</c>.</exception>
   Public Shared Function GetUnsignedLong(fromInclusive As ULong,
                                          toInclusive As ULong,
                                          getUnsignedLongBasicFunction As GetUnsignedLongBasicCallback) As ULong
      If getUnsignedLongBasicFunction Is Nothing Then _
         Throw New ArgumentNullException(NameOf(getUnsignedLongBasicFunction))

      Dim result As ULong

      If toInclusive >= fromInclusive Then
         If (toInclusive < ULong.MaxValue) OrElse (fromInclusive > 0) Then
            ' Calculate the size of the interval that should be returned
            Dim maxValue As ULong = toInclusive - fromInclusive  ' This is always nonnegative
            Dim size As ULong = maxValue + 1UL                   ' This is always positive

            ' If the size is a power of 2 we are done
            If (size And maxValue) = 0UL Then
               result = getUnsignedLongBasicFunction() And maxValue
            Else
               ' Size is not a power of two, so we need to calculate a pseudo-random
               ' number that is not biased

               ' Calculate the mask for the smallest power of two that is larger than maxValue
               Dim mask As ULong = GetUnsignedLongMaskForValue(maxValue)

               ' Now get a random number with the mask laid over it and reject all values that are too large
               Do
                  result = getUnsignedLongBasicFunction() And mask
               Loop While result > maxValue
            End If
         Else
            ' We only get here if 'fromInclusive = MinValue' and 'toInclusive = MaxValue'. I.e. get a number without any boundaries.
            result = getUnsignedLongBasicFunction()
         End If
      Else
         ThrowRangeException()
      End If

      ' Return the calculated pseudo-random number in the interval plus the minimum value
      Return result + fromInclusive
   End Function

   ''' <summary>
   ''' Get a pseudo-random long in a range.
   ''' </summary>
   ''' <remarks>
   ''' This method returns a signed number but the delegate has to be the one of the <em>unsigned</em> type.
   ''' </remarks>
   ''' <param name="fromInclusive">Inclusive start point of the range.</param>
   ''' <param name="toInclusive">Inclusive end point of the range.</param>
   ''' <param name="getUnsignedLongBasicFunction">Delegate of function that gets one unsigned long.</param>
   ''' <returns>Pseudo-random long that has a value between <paramref name="fromInclusive"/> and <paramref name="toInclusive"/>.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="fromInclusive"/> is larger than <paramref name="toInclusive"/>.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="getUnsignedLongBasicFunction"/> is <c>Nothing</c>.</exception>
   Public Shared Function GetLong(fromInclusive As Long,
                                  toInclusive As Long,
                                  getUnsignedLongBasicFunction As GetUnsignedLongBasicCallback) As Long
      If getUnsignedLongBasicFunction Is Nothing Then _
         Throw New ArgumentNullException(NameOf(getUnsignedLongBasicFunction))

      Dim result As Long

      If toInclusive >= fromInclusive Then
         '
         ' We need to work with the unsigned data type here as this is the only way to correctly work with the mask method.
         ' Negative numbers can not be used with masks. So we calculate the span of the boundaries, get an unsigned result
         ' in that span and add this result to the "from" boundary.
         '
         Dim unsignedSpan As ULong

         If (fromInclusive < 0L) AndAlso (toInclusive >= 0L) Then
            unsignedSpan = CULng(Not fromInclusive) + 1UL + CULng(toInclusive)   ' Add the negative number in a way that can not overflow
         Else
            unsignedSpan = CULng(toInclusive - fromInclusive)
         End If

         Dim unsignedSpanResult As ULong = GetUnsignedLong(0UL, unsignedSpan, getUnsignedLongBasicFunction)

         '
         ' Now we are in signed/unsigned hell: We can not add an unsigned number that is larger than the
         ' maximum value of the signed type. We have to check for this situation.
         '
         If unsignedSpanResult <= LONG_MAX_VALUE_AS_ULONG Then
            ' This is the easy part: Just add the number.
            result = fromInclusive + CLng(unsignedSpanResult)
         Else
            ' If the unsigned number is larger than the maximum value of the signed type we first add the
            ' maximum number and then the rest. This will never overflow.
            result = fromInclusive + Long.MaxValue
            result += CLng(unsignedSpanResult - LONG_MAX_VALUE_AS_ULONG)
         End If
      Else
         ThrowRangeException()
      End If

      Return result
   End Function
#End Region

#Region "Private methods"
#Region "Mask helpers"
   ''' <summary>
   ''' Get a bit mask that covers the bits of the supplied byte value.
   ''' </summary>
   ''' <param name="aValue">Value to get the mask for.</param>
   ''' <returns>Byte mask that covers the bits of the supplied value.</returns>
   Private Shared Function GetByteMaskForValue(aValue As Byte) As Byte
      Dim result As Byte = ZERO_AS_BYTE

      Dim mask As Byte = ONE_AS_BYTE

      Do While aValue >= mask
#Disable Warning S2437 ' Silly bit operations should not be performed: Suppress this silly warning
         result = (result Or mask)
#Enable Warning S2437 ' Silly bit operations should not be performed

         If mask = MSB_AS_BYTE Then _
            Exit Do

         mask <<= 1
      Loop

      ' If the value is 0 the result has to be corrected to be 1 bit
      If result = ZERO_AS_BYTE Then _
         result = ONE_AS_BYTE

      Return result
   End Function

   ''' <summary>
   ''' Get an unsigned short bit mask that covers the bits of the supplied value.
   ''' </summary>
   ''' <param name="aValue">Value to get the mask for.</param>
   ''' <returns>Unsigned short mask that covers the bits of the supplied value.</returns>
   Private Shared Function GetUnsignedShortMaskForValue(aValue As UShort) As UShort
      Dim result As UShort = 0US

      Dim mask As UShort = 1US

      Do While aValue >= mask
#Disable Warning S2437 ' Silly bit operations should not be performed: Suppress this silly warning
         result = (result Or mask)
#Enable Warning S2437 ' Silly bit operations should not be performed

         If mask = &H8000US Then _
            Exit Do

         mask <<= 1
      Loop

      ' If the value is 0 the result has to be corrected to be 1 bit
      If result = 0US Then _
         result = 1US

      Return result
   End Function

   ''' <summary>
   ''' Get an unsigned integer bit mask that covers the bits of the supplied value.
   ''' </summary>
   ''' <param name="aValue">Value to get the mask for.</param>
   ''' <returns>Unsigned integer mask that covers the bits of the supplied value.</returns>
   Private Shared Function GetUnsignedIntegerMaskForValue(aValue As UInteger) As UInteger
      Dim result As UInteger = 0UI

      Dim mask As UInteger = 1UI

      Do While aValue >= mask
#Disable Warning S2437 ' Silly bit operations should not be performed: Suppress this silly warning
         result = (result Or mask)
#Enable Warning S2437 ' Silly bit operations should not be performed

         If mask = &H80000000UI Then _
            Exit Do

         mask <<= 1
      Loop

      ' If the value is 0 the result has to be corrected to be 1 bit
      If result = 0UI Then _
         result = 1UI

      Return result
   End Function

   ''' <summary>
   ''' Get an unsigned long bit mask that covers the bits of the supplied value.
   ''' </summary>
   ''' <param name="aValue">Value to get the mask for.</param>
   ''' <returns>Unsigned long mask that covers the bits of the supplied value.</returns>
   Private Shared Function GetUnsignedLongMaskForValue(aValue As ULong) As ULong
      Dim result As ULong = 0

      Dim mask As ULong = 1

      Do While aValue >= mask
#Disable Warning S2437 ' Silly bit operations should not be performed: Suppress this silly warning
         result = (result Or mask)
#Enable Warning S2437 ' Silly bit operations should not be performed

         If mask = &H8000000000000000UL Then _
            Exit Do

         mask <<= 1
      Loop

      ' If the value is 0 the result has to be corrected to be 1 bit
      If result = 0UL Then _
         result = 1UL

      Return result
   End Function
#End Region

#Region "Exception helpers"
   ''' <summary>
   ''' Throw an argument exception if 'fromInclusive' is larger than 'toInclusive'.
   ''' </summary>
   ''' <exception cref="ArgumentException">Always thrown as this is the purpose of this method.</exception>
   Private Shared Sub ThrowRangeException()
      Throw New ArgumentException("'fromInclusive' is larger than 'toInclusive'")
   End Sub
#End Region
#End Region
End Class
