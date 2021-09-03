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
' Version: 3.1.1
'
' Change history:
'    2020-04-23: V1.0.0: Created.
'    2020-05-18: V1.0.1: Use lock object for Dispose.
'    2020-05-18: V1.0.2: Instantiate lock object.
'    2020-12-08: V1.0.3: Explain usage of IndexOutOfRangeException.
'    2020-12-10: V2.0.0: Throw ObjectDisposedException instead of InvalidOperationException.
'    2020-12-11: V2.0.1: Put IsValid method where it belongs.
'    2020-12-14: V2.0.2: Corrected some comments.
'    2020-12-16: V2.0.3: Made usage of SyncLock for disposal consistent.
'    2021-06-08: V3.0.0: Byte array is protected by an index dependent masker now. No more need for an obfuscation array.
'    2021-06-09: V3.0.1: Fixed wrong hash code calculation when only part of a source array was protected.
'    2021-09-01: V3.1.0: Added forgotten Dispose of the index masker.
'    2021-09-03: V3.1.1: Fortify finding: Added forgotten check for maximum count of source array.
'

''' <summary>
''' Stores a byte array in a protected and obfuscated form.
''' </summary>
Public Class ProtectedByteArray : Implements IDisposable

#Region "Private constants"
   '******************************************************************
   ' Private constants
   '******************************************************************
   Private Const CLASS_HASH_SEED_1 As UInteger = 314159265UI
   Private Const CLASS_HASH_SEED_2 As UInteger = 271828182UI

   ''' <summary>
   ''' Arrays are stored in blocks of this size.
   ''' </summary>
   Private Const INDEX_BLOCK_SIZE As Integer = 50

   ''' <summary>
   ''' Maximum length for a source array.
   ''' </summary>
   Private Const MAX_SOURCE_ARRAY_LENGTH = (Integer.MaxValue \ INDEX_BLOCK_SIZE) * INDEX_BLOCK_SIZE

   ' Pro forma indices for special data.
   ' They can have any negative value

   ''' <summary>
   ''' Pro forma index for the data length.
   ''' </summary>
   Private Const INDEX_LENGTH = -3

   ''' <summary>
   ''' Pro forma index for the start index.
   ''' </summary>
   Private Const INDEX_START = -97
#End Region

#Region "Instance variables"
   '******************************************************************
   ' Instance variables
   '******************************************************************

   ''' <summary>
   ''' Obfuscated data.
   ''' </summary>
   Private m_ByteArray As Byte()

   ''' <summary>
   ''' Index array into <see cref="m_ByteArray"/>.
   ''' </summary>
   Private m_IndexArray As Integer()

   ''' <summary>
   ''' Start index in index array in obfuscated form.
   ''' </summary>
   Private m_IndexStart As Integer

   ''' <summary>
   ''' Length of data in <see cref="m_ByteArray"/> in obfuscated form.
   ''' </summary>
   Private m_StoredArrayLength As Integer

   ''' <summary>
   ''' Hash code of data in <see cref="m_ByteArray"/>.
   ''' </summary>
   Private m_HashCode As Integer

   ''' <summary>
   ''' Indicator of data has changed so the hash code needs to be recalculated.
   ''' </summary>
   Private m_HasChanged As Boolean

   ''' <summary>
   ''' Index masker to use.
   ''' </summary>
   Private m_IndexMasker As MaskedIndex

   ''' <summary>
   ''' Object only used for locking the call to Dispose.
   ''' </summary>
   Private ReadOnly m_LockObject As New Object
#End Region

#Region "Constructors"
   '******************************************************************
   ' Constructors
   '******************************************************************

   ''' <summary>
   ''' Constructor for the protected byte array with a source array.
   ''' </summary>
   ''' <param name="sourceArray">Source byte array</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="sourceArray"/> is <c>Nothing</c>.</exception>
   Public Sub New(sourceArray As Byte())
      Me.New(sourceArray, 0)
   End Sub

   ''' <summary>
   ''' Constructor for the protected byte array with a source array and an offset into that source array.
   ''' </summary>
   ''' <param name="sourceArray">Source byte array</param>
   ''' <param name="offset">Offset in the source to get data from.</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="sourceArray"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="offset"/> is less than 0 or larger than the upper bound
   ''' of <paramref name="sourceArray"/>.</exception>
   Public Sub New(sourceArray As Byte(), offset As Integer)
      RequireNonNull(sourceArray, NameOf(sourceArray))

      InitializeInstance(sourceArray, offset, sourceArray.Length - offset)
   End Sub

   ''' <summary>
   ''' Constructor for the protected byte array with a source array, an offset and a count.
   ''' </summary>
   ''' <param name="sourceArray">Source byte array</param>
   ''' <param name="offset">Offset in the source to get data from.</param>
   ''' <param name="count">Number of bytes to copy from the source array.</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="sourceArray"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="offset"/> is less than 0 or larger than the upper bound
   ''' of <paramref name="sourceArray"/> or <paramref name="count"/> is less than 0 or <paramref name="offset"/> plus 
   ''' <paramref name="count"/> is too large to fit in <paramref name="sourceArray"/>.</exception>
   Public Sub New(sourceArray As Byte(), offset As Integer, count As Integer)
      RequireNonNull(sourceArray, NameOf(sourceArray))

      InitializeInstance(sourceArray, offset, count)
   End Sub
#End Region

#Region "Public methods"
   '******************************************************************
   ' Public methods
   '******************************************************************

#Region "Access methods"
   '
   ' Access methods
   '

   ''' <summary>
   ''' Gets the original array content.
   ''' </summary>
   ''' <exception cref="ObjectDisposedException">Thrown when the <see cref="ProtectedByteArray"/> has already been disposed of.</exception>
   ''' <returns>Original array content</returns>
   Public Function GetData() As Byte()
      CheckState()

      Return GetValues()
   End Function

   ''' <summary>
   ''' Element at a given position.
   ''' </summary>
   ''' <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="externalIndex"/> is not valid.</exception>
   ''' <exception cref="ObjectDisposedException">Thrown when the <see cref="ProtectedByteArray"/> has already been disposed of.</exception>
   ''' <returns>>Value of the array element at the given position</returns>
   Public Property ElementAt(externalIndex As Integer) As Byte
      Get
         CheckStateAndExternalIndex(externalIndex)

         Return m_IndexMasker.GetByteMask(externalIndex) Xor m_ByteArray(GetArrayIndex(externalIndex))
      End Get

      Set(newValue As Byte)
         CheckStateAndExternalIndex(externalIndex)

         m_ByteArray(GetArrayIndex(externalIndex)) = m_IndexMasker.GetByteMask(externalIndex) Xor newValue

         m_HasChanged = True
      End Set
   End Property

   ''' <summary>
   ''' Length of data stored in the array.
   ''' </summary>
   ''' <exception cref="ObjectDisposedException">Thrown when the <see cref="ProtectedByteArray"/> has already been disposed of.</exception>
   ''' <returns>Real length of stored array</returns>
   Public ReadOnly Property Length As Integer
      Get
         CheckState()

         Return GetRealLength()
      End Get
   End Property
#End Region

#Region "Overridden Object methods"
   ''' <summary>
   ''' Returns the hash code of this instance.
   ''' </summary>
   ''' <exception cref="ObjectDisposedException">Thrown when the instance has already been disposed of.</exception>
   ''' <returns>The hash code.</returns>
   Public Overrides Function GetHashCode() As Integer
      CheckState()

      If m_HasChanged Then _
         CalculateHashCode()

      Return m_HashCode
   End Function

   ''' <summary>
   ''' Compares the specified object with this instance.
   ''' </summary>
   ''' <param name="obj">The object to compare.</param>
   ''' <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed of.</exception>
   ''' <returns><c>true</c> if byte arrays of both object are equal, otherwise <c>false</c>}.</returns>
   Public Overrides Function Equals(obj As Object) As Boolean
      Dim result As Boolean = False

      If TypeOf obj Is ProtectedByteArray Then
         Dim other As ProtectedByteArray = DirectCast(obj, ProtectedByteArray)

         Dim thisClearArray As Byte() = GetData()
         Dim otherClearArray As Byte() = other.GetData()

         result = ArrayHelper.SecureAreEqual(thisClearArray, otherClearArray)

         ArrayHelper.Clear(thisClearArray)
         ArrayHelper.Clear(otherClearArray)
      End If

      Return result
   End Function
#End Region
#End Region

#Region "Private methods"
   '******************************************************************
   ' Private methods
   '******************************************************************

#Region "Check methods"
   '
   ' Check methods
   '

   ''' <summary>
   ''' Throws an exception if this instance is not in a valid state.
   ''' </summary>
   ''' <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed of.</exception>
   Private Sub CheckState()
      If m_IsDisposed Then _
         Throw New ObjectDisposedException(NameOf(ProtectedByteArray))
   End Sub

   ''' <summary>
   ''' Check whether <paramref name="offset"/> and <paramref name="count"/> are valid and fit into the <paramref name="sourceArray"/>.
   ''' </summary>
   ''' <param name="sourceArray">Source array to be checked against.</param>
   ''' <param name="offset">Offset of data in <paramref name="sourceArray"/>.</param>
   ''' <param name="count">Count of data.</param>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="offset"/> and <paramref name="count"/> are  
   ''' incompatible with <paramref name="sourceArray"/> or <paramref name="count"/> is too large to fit in
   ''' <paramref name="sourceArray"/>.</exception>
   ''' <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> or <paramref name="count"/> are
   ''' less than 0 or <paramref name="offset"/> is larger than the length of <paramref name="sourceArray"/>.</exception>
   Private Shared Sub CheckOffsetAndLength(sourceArray As Byte(), offset As Integer, count As Integer)
      If offset < 0 Then _
         Throw New ArgumentOutOfRangeException(NameOf(offset))

      '
      ' A source array of length 0 has a lower bound of 0 and an upper bound of -1. So we must not check the offset
      ' if the source array has a length of 0.
      '
      If sourceArray.Length > 0 AndAlso offset >= sourceArray.Length Then _
         Throw New ArgumentOutOfRangeException(NameOf(offset))

      If (count < 0) OrElse (count > MAX_SOURCE_ARRAY_LENGTH) Then _
         Throw New ArgumentOutOfRangeException(NameOf(count))

      If (offset + count) > sourceArray.Length Then _
            Throw New ArgumentException("Array is not large enough for count elements from offset")
   End Sub

   ''' <summary>
   ''' Checks whether a given external index is valid.
   ''' </summary>
   ''' <remarks>
   ''' This methods throws an <see cref="IndexOutOfRangeException"/> which is a runtime exception that should normally
   ''' not be thrown by user code. However, here an index operation is performend in user code and this exception is the correct one.
   ''' </remarks>
   ''' <param name="externalIndex">Index value to be checked</param>
   ''' <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="externalIndex"/> is not valid.</exception>
   Private Sub CheckExternalIndex(externalIndex As Integer)
#Disable Warning S112  ' General exceptions should not be thrown by user code
      If (externalIndex < 0) OrElse
         (externalIndex >= GetRealLength()) Then _
         Throw New IndexOutOfRangeException(NameOf(externalIndex))
#Enable Warning S112  ' General exceptions should not be thrown by user code
   End Sub

   ''' <summary>
   ''' Checks first the state and then the validity of the given external index.
   ''' </summary>
   ''' <param name="externalIndex">Index value to be checked</param>
   ''' <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="externalIndex"/> is not valid.</exception>
   ''' <exception cref="ObjectDisposedException">Thrown when the <see cref="ProtectedByteArray"/> has already been disposed of.</exception>
   Private Sub CheckStateAndExternalIndex(externalIndex As Integer)
      CheckState()

      CheckExternalIndex(externalIndex)
   End Sub
#End Region

#Region "Methods for data structure initialization and maintenance"
   ''' <summary>
   ''' Calculates the array size required for storing the data. It is a multiple of <see cref="INDEX_BLOCK_SIZE"/>.
   ''' </summary>
   ''' <param name="forSize">Original size</param>
   ''' <returns>Size of store array</returns>
   Private Shared Function CalculateStoreLength(forSize As Integer) As Integer
      Dim padLength = INDEX_BLOCK_SIZE - (forSize Mod INDEX_BLOCK_SIZE)

      Return forSize + padLength
   End Function

   ''' <summary>
   ''' Initializes the index array with each position holding it's own index in  store index form.
   ''' </summary>
   Private Sub InitializeIndexArray()
      For i As Integer = 0 To m_IndexArray.Length - 1
         m_IndexArray(i) = i
      Next
   End Sub

   ''' <summary>
   ''' Shuffles the positions in the index array.
   ''' </summary>
   Private Sub ShuffleIndexArray()
      Dim i1 As Integer
      Dim i2 As Integer
      Dim swapIndex As Integer

      Dim nextIndex As Integer = 0

      Dim arrayLength As Integer = m_IndexArray.Length

      Do
         i1 = SecurePseudoRandomNumberGenerator.GetInteger(arrayLength)
         i2 = SecurePseudoRandomNumberGenerator.GetInteger(arrayLength)

         If i1 <> i2 Then
            swapIndex = m_IndexArray(i1)
            m_IndexArray(i1) = m_IndexArray(i2)
            m_IndexArray(i2) = swapIndex

            nextIndex += 1
         End If
      Loop Until nextIndex >= arrayLength
   End Sub

   ''' <summary>
   ''' Mask all index array entries.
   ''' </summary>
   Private Sub MaskIndexArray()
      For i As Integer = 0 To m_IndexArray.Length - 1
         m_IndexArray(i) = m_IndexArray(i) Xor m_IndexMasker.GetIntegerMask(i)
      Next i
   End Sub

   ''' <summary>
   ''' Sets up the index array by initializing and shuffling it.
   ''' </summary>
   Private Sub SetUpIndexArray()
      InitializeIndexArray()

      ShuffleIndexArray()

      MaskIndexArray()
   End Sub


   ''' <summary>
   ''' Initialize this instance.
   ''' </summary>
   ''' <remarks>After successful completion this instamce is marked as <c>valid</c>.</remarks>
   ''' <param name="sourceArray">Source byte array.</param>
   ''' <param name="offset">Offset in the source to get data from.</param>
   ''' <param name="count">Number of bytes to copy from the source array.</param>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="offset"/> is less than 0 or larger than the upper bound
   ''' of <paramref name="sourceArray"/> or <paramref name="count"/> is less than 0 or <paramref name="offset"/> plus 
   ''' <paramref name="count"/> is too large to fit in <paramref name="sourceArray"/>.</exception>
   Private Sub InitializeInstance(sourceArray As Byte(), offset As Integer, count As Integer)
      CheckOffsetAndLength(sourceArray, offset, count)

      InitializeDataStructures(count)

      SetValues(sourceArray, offset, count)

      CalculateHashCode()
   End Sub

   ''' <summary>
   ''' Allocates and initializes all necessary data structures.
   ''' </summary>
   ''' <param name="sourceLength">Length of source array.</param>
   Private Sub InitializeDataStructures(sourceLength As Integer)
      m_IndexMasker = New MaskedIndex

      Dim storeLength As Integer = CalculateStoreLength(sourceLength)

      m_ByteArray = New Byte(0 To storeLength - 1) {}

      SecurePseudoRandomNumberGenerator.GetBytes(m_ByteArray)

      m_IndexArray = New Integer(0 To storeLength - 1) {}

      SetUpIndexArray()

      m_IndexStart = ConvertIndex(CalculateStartIndex(sourceLength, storeLength), INDEX_START)

      m_StoredArrayLength = ConvertIndex(sourceLength, INDEX_LENGTH)
   End Sub

   ''' <summary>
   ''' Calculates the start index into the index array.
   ''' </summary>
   ''' <param name="sourceLength">The real data length.</param>
   ''' <param name="storeLength">The length of the stored data.</param>
   ''' <returns></returns>
   Private Shared Function CalculateStartIndex(sourceLength As Integer, storeLength As Integer) As Integer
      Dim supStart As Integer = storeLength - sourceLength + 1

      If supStart > 1 Then
         Return SecurePseudoRandomNumberGenerator.GetInteger(supStart)
      Else
         Return 0
      End If
   End Function

   ''' <summary>
   ''' Clear all data.
   ''' </summary>
   Private Sub ClearData()
      m_HashCode = 0

      m_StoredArrayLength = 0

      m_IndexStart = 0

      m_HasChanged = False

      ArrayHelper.Clear(m_ByteArray)
      m_ByteArray = Nothing

      ArrayHelper.Clear(m_IndexArray)
      m_IndexArray = Nothing
   End Sub
#End Region

#Region "Index conversion methods"
   ''' <summary>
   ''' Converts an index between true and masked value.
   ''' </summary>
   ''' <param name="sourceIndex">The index in the index array.</param>
   ''' <param name="forPosition">The possibly different position value.</param>
   ''' <returns>The converted index value.</returns>
   Private Function ConvertIndex(sourceIndex As Integer, forPosition As Integer) As Integer
      Return m_IndexMasker.GetIntegerMask(forPosition) Xor sourceIndex
   End Function

   ''' <summary>
   ''' Gets the array index from the external index.
   ''' </summary>
   ''' <param name="externalIndex">External index.</param>
   ''' <returns>Index into the byte array.</returns>
   Private Function GetArrayIndex(externalIndex As Integer) As Integer
      Dim position As Integer = externalIndex + ConvertIndex(m_IndexStart, INDEX_START)

      Return ConvertIndex(m_IndexArray(position), position)
   End Function

#End Region

#Region "Data access methods"
   ''' <summary>
   ''' Gets the unobfuscated data length.
   ''' </summary>
   ''' <returns>True data length</returns>
   Private Function GetRealLength() As Integer
      Return ConvertIndex(m_StoredArrayLength, INDEX_LENGTH)
   End Function

   '
   ' Methods for accessing data from or to the byte array
   '

   ''' <summary>
   ''' Sets the destination array to the values in the source array.
   ''' </summary>
   ''' <param name="sourceArray">Source byte array.</param>
   ''' <param name="offset">Start offset in source array.</param>
   Private Sub SetValues(sourceArray As Byte(), offset As Integer, count As Integer)
      Dim sourceIndex As Integer = offset

      For i As Integer = 0 To count - 1
         m_ByteArray(GetArrayIndex(i)) = m_IndexMasker.GetByteMask(i) Xor sourceArray(sourceIndex)

         sourceIndex += 1
      Next
   End Sub

   ''' <summary>
   ''' Gets the values from the protected array. 
   ''' </summary>
   ''' <returns>Values stored in the protected byte array.</returns>
   Private Function GetValues() As Byte()
      Dim result As Byte()

      result = New Byte(0 To GetRealLength() - 1) {}

      For i As Integer = 0 To result.Length - 1
         result(i) = m_IndexMasker.GetByteMask(i) Xor m_ByteArray(GetArrayIndex(i))
      Next

      Return result
   End Function

   ''' <summary>
   ''' Calculates the hash code of the data.
   ''' </summary>
   Private Sub CalculateHashCode()
      Dim content As Byte() = GetValues()

      Dim aHasher As New SimpleHasher(CLASS_HASH_SEED_1, CLASS_HASH_SEED_2)

      For i As Integer = 0 To content.Length - 1
         aHasher.UpdateHash(content(i))
      Next

      ArrayHelper.Clear(content)

      m_HashCode = aHasher.GetHashValue()

      m_HasChanged = False
   End Sub
#End Region

#Region "Exception helper methods"
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

#Region "IDisposable Support"
   ''' <summary>
   ''' Marker, if disposition of managed resources has already been done.
   ''' </summary>
   Private m_IsDisposed As Boolean = False

   ''' <summary>
   ''' Dispose managed and unmanged resources.
   ''' </summary>
   ''' <param name="disposeManagedResources"><c>true</c>, if managed resource are to be disposed of, <c>false</c>, if not.</param>
   Protected Overridable Sub Dispose(disposeManagedResources As Boolean)
      '
      ' Disposing of resources needs to be synchronized to prevent a race condition.
      '
      SyncLock m_LockObject
         If Not m_IsDisposed Then
            m_IsDisposed = True

            If disposeManagedResources Then
               ClearData()

               If m_IndexMasker IsNot Nothing Then
                  m_IndexMasker.Dispose()
                  m_IndexMasker = Nothing
               End If
            End If

            ' Free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' Set large fields to null.
         End If
      End SyncLock
   End Sub

   ' Override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
   'Protected Overrides Sub Finalize()
   '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
   '    Dispose(False)
   '    MyBase.Finalize()
   'End Sub

   ''' <summary>
   ''' Dispose of resources.
   ''' </summary>
   ''' <remarks>
   ''' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
   ''' </remarks>
   Public Sub Dispose() Implements IDisposable.Dispose
      Dispose(True)
      ' Uncomment the following line if Finalize() is overridden above.
      ' GC.SuppressFinalize(Me)
   End Sub

   ''' <summary>
   ''' Checks whether this instance is valid
   ''' </summary>
   ''' <returns><c>true</c>, if this instance is in a valid state, <c>false</c>, if this instance has already been disposed of.</returns>
   Public ReadOnly Property IsValid As Boolean
      Get
         SyncLock m_LockObject
            Return Not m_IsDisposed
         End SyncLock
      End Get
   End Property
#End Region
End Class
