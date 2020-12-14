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
' Version: 2.0.2
'
' Change history:
'    2020-04-23: V1.0.0: Created.
'    2020-05-18: V1.0.1: Use lock object for Dispose.
'    2020-05-18: V1.0.2: Instantiate lock object.
'    2020-12-08: V1.0.3: Explain usage of IndexOutOfRangeException.
'    2020-12-10: V2.0.0: Throw ObjectDisposedException instead of InvalidOperationException.
'    2020-12-11: V2.0.1: Put IsValid method where it belongs.
'    2020-12-14: V2.0.2: Corrected some comments.
'

''' <summary>
''' Stores a byte array in a shuffled form.
''' </summary>
Public Class ShuffledByteArray : Implements IDisposable

#Region "Private constants"
   '******************************************************************
   ' Private constants
   '******************************************************************
   Private Const CLASS_HASH_SEED_1 As UInteger = 314159265UI
   Private Const CLASS_HASH_SEED_2 As UInteger = 271828182UI
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
   ''' Index array into <c>m_ByteArray</c>.
   ''' </summary>
   Private m_IndexArray As Integer()

   ''' <summary>
   ''' Offset for index calculation.
   ''' </summary>
   Private m_IndexOffset As Integer

   ''' <summary>
   ''' Factor for index calculation.
   ''' </summary>
   Private m_IndexFactor As Integer

   ''' <summary>
   ''' Start index in index array.
   ''' </summary>
   Private m_IndexStart As Integer

   ''' <summary>
   ''' Length of data in <c>m_ByteArray</c> in obfuscated form.
   ''' </summary>
   Private m_StoredArrayLength As Integer

   ''' <summary>
   ''' Hash code of data in <c>m_ByteArray</c>.
   ''' </summary>
   Private m_HashCode As Integer

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
   ''' Constructor for the shuffled byte array with a source array, an offset and a count.
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

      BuildShuffledData(sourceArray, offset, count)
   End Sub

   ''' <summary>
   ''' Constructor for the shuffled byte array with a source array and an offset into that source array.
   ''' </summary>
   ''' <param name="sourceArray">Source byte array</param>
   ''' <param name="offset">Offset in the source to get data from.</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="sourceArray"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="offset"/> is less than 0 or larger than the upper bound
   ''' of <paramref name="sourceArray"/>.</exception>
   Public Sub New(sourceArray As Byte(), offset As Integer)
      RequireNonNull(sourceArray, NameOf(sourceArray))

      BuildShuffledData(sourceArray, offset, sourceArray.Length - offset)
   End Sub

   ''' <summary>
   ''' Constructor for the shuffled byte array with a source array
   ''' </summary>
   ''' <param name="sourceArray">Source byte array</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="sourceArray"/> is <c>Nothing</c>.</exception>
   Public Sub New(sourceArray As Byte())
      Me.New(sourceArray, 0)
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
   ''' Gets the original array content
   ''' </summary>
   ''' <exception cref="ObjectDisposedException">Thrown when the <see cref="ShuffledByteArray"/> has already been disposed of.</exception>
   ''' <returns>Original array content</returns>
   Public Function GetData() As Byte()
      CheckState()

      Return GetValues()
   End Function

   ''' <summary>
   ''' Element at a given position
   ''' </summary>
   ''' <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="externalIndex"/> is not valid.</exception>
   ''' <exception cref="ObjectDisposedException">Thrown when the <see cref="ShuffledByteArray"/> has already been disposed of.</exception>
   ''' <returns>>Value of the array element at the given position</returns>
   Public Property ElementAt(externalIndex As Integer) As Byte
      Get
         CheckStateAndExternalIndex(externalIndex)

         Return m_ByteArray(GetArrayIndex(externalIndex))
      End Get

      Set(newValue As Byte)
         CheckStateAndExternalIndex(externalIndex)

         m_ByteArray(GetArrayIndex(externalIndex)) = newValue
      End Set
   End Property

   ''' <summary>
   ''' Length of data stored in the array
   ''' </summary>
   ''' <exception cref="ObjectDisposedException">Thrown when the <see cref="ShuffledByteArray"/> has already been disposed of.</exception>
   ''' <returns>Real length of stored array</returns>
   Public ReadOnly Property Length As Integer
      Get
         CheckState()

         Return GetRealIndex(m_StoredArrayLength)
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

      If TypeOf obj Is ShuffledByteArray Then
         Dim other As ShuffledByteArray = DirectCast(obj, ShuffledByteArray)

         If Length = other.Length Then
            Dim thisClearArray As Byte() = GetData()
            Dim otherClearArray As Byte() = other.GetData()

            result = ArrayHelper.SecureAreEqual(thisClearArray, otherClearArray)

            ArrayHelper.Clear(thisClearArray)
            ArrayHelper.Clear(otherClearArray)
         End If
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
   ''' Throws an exception if this instance is not in a valid state
   ''' </summary>
   ''' <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed of.</exception>
   Private Sub CheckState()
      If m_IsDisposed Then _
         Throw New ObjectDisposedException(NameOf(ShuffledByteArray))
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

      If count < 0 Then _
         Throw New ArgumentOutOfRangeException(NameOf(count))

      If (offset + count) > sourceArray.Length Then _
         Throw New ArgumentException("Array is not large enough for count elements from offset")
   End Sub

   ''' <summary>
   ''' Checks whether a given external index is valid
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
         (externalIndex >= GetRealIndex(m_StoredArrayLength)) Then _
         Throw New IndexOutOfRangeException(NameOf(externalIndex))
#Enable Warning S112  ' General exceptions should not be thrown by user code
   End Sub

   ''' <summary>
   ''' Checks first the state and then the validity of the given external index
   ''' </summary>
   ''' <param name="externalIndex">Index value to be checked</param>
   ''' <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="externalIndex"/> is not valid.</exception>
   ''' <exception cref="ObjectDisposedException">Thrown when the <see cref="ShuffledByteArray"/> has already been disposed of.</exception>
   Private Sub CheckStateAndExternalIndex(externalIndex As Integer)
      CheckState()

      CheckExternalIndex(externalIndex)
   End Sub
#End Region

#Region "Methods for data structure initialization and maintenance"
   ''' <summary>
   ''' Calculates the array size required for storing the data. The stored array
   ''' has at least twice the size of the original array to be able to set a
   ''' random start point in the index reorder array.
   ''' </summary>
   ''' <param name="forSize">Original size</param>
   ''' <returns>Size of shuffled array</returns>
   Private Shared Function GetStoreLength(forSize As Integer) As Integer
      Dim calcSize As Integer = forSize + forSize + 7

      Return SecurePseudoRandomNumberGenerator.GetInteger(calcSize) + calcSize
   End Function

   ''' <summary>
   ''' Gets the offset for index obfuscation
   ''' </summary>
   ''' <param name="arrayLength">Length of the array</param>
   ''' <returns>Offset for indices</returns>
   Private Shared Function GetIndexOffset(arrayLength As Integer) As Integer
      Return SecurePseudoRandomNumberGenerator.GetInteger(10000) + arrayLength + arrayLength + 1
   End Function

   ''' <summary>
   ''' Gets the factor for index obfuscation.
   ''' </summary>
   ''' <param name="offset">Offset for indices</param>
   ''' <param name="arrayLength">Length of the array</param>
   ''' <returns>Factor for index obfuscation</returns>
   Private Shared Function GetIndexFactor(offset As Integer, arrayLength As Integer) As Integer
      Return (5 * offset) \ (3 * arrayLength)
   End Function

   ''' <summary>
   ''' Initializes the index array with each position holding it's own index in  store index form.
   ''' </summary>
   Private Sub InitializeIndexArray()
      For i As Integer = 0 To m_IndexArray.Length - 1
         m_IndexArray(i) = GetStoreIndex(i)
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
   ''' Gets the start position in an array.
   ''' </summary>
   ''' <param name="arrayLength">Length of the array to get the start position for.</param>
   ''' <returns>Start position in the array.</returns>
   Private Shared Function GetStartPosition(arrayLength As Integer) As Integer
      ' "+1" because the max. start position is at the half size of
      ' the array.
      Return SecurePseudoRandomNumberGenerator.GetInteger(0, arrayLength >> 1)
   End Function

   ''' <summary>
   '''  Reorganizes the index array for reordering of the byte array.
   ''' </summary>
   ''' <remarks>
   ''' This includes setting a random start position in the index array.
   ''' </remarks>
   Private Sub ReorganizeIndexArray()
      ShuffleIndexArray()

      m_IndexStart = GetStartPosition(m_IndexArray.Length)
   End Sub

   ''' <summary>
   ''' Sets up the index array by initializing and shuffling it.
   ''' </summary>
   Private Sub SetUpIndexArray()
      InitializeIndexArray()

      ReorganizeIndexArray()
   End Sub


   ''' <summary>
   ''' Build data structures for shuffled byte array.
   ''' </summary>
   ''' <remarks>After successful completion this instamce is marked as <c>valid</c>.</remarks>
   ''' <param name="sourceArray">Source byte array.</param>
   ''' <param name="offset">Offset in the source to get data from.</param>
   ''' <param name="count">Number of bytes to copy from the source array.</param>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="offset"/> is less than 0 or larger than the upper bound
   ''' of <paramref name="sourceArray"/> or <paramref name="count"/> is less than 0 or <paramref name="offset"/> plus 
   ''' <paramref name="count"/> is too large to fit in <paramref name="sourceArray"/>.</exception>
   Private Sub BuildShuffledData(sourceArray As Byte(), offset As Integer, count As Integer)
      CheckOffsetAndLength(sourceArray, offset, count)

      InitializeDataStructures(count)

      SetValues(sourceArray, offset, count)
   End Sub

   ''' <summary>
   ''' Allocates and initializes all necessary arrays.
   ''' </summary>
   ''' <param name="sourceLength">Length of source array.</param>
   Private Sub InitializeDataStructures(sourceLength As Integer)
      Dim storeLength As Integer = GetStoreLength(sourceLength)

      m_ByteArray = New Byte(0 To storeLength - 1) {}
      SecurePseudoRandomNumberGenerator.GetBytes(m_ByteArray)   ' Initialize the data with random values

      m_IndexArray = New Integer(0 To storeLength - 1) {}

      m_IndexOffset = GetIndexOffset(storeLength)
      m_IndexFactor = GetIndexFactor(m_IndexOffset, storeLength)
      SetUpIndexArray()

      m_StoredArrayLength = GetStoreIndex(sourceLength)
   End Sub

   ''' <summary>
   ''' Clear all data.
   ''' </summary>
   Private Sub ClearData()
      ArrayHelper.Clear(m_ByteArray)
      ArrayHelper.Clear(m_IndexArray)

      m_IndexStart = 0
      m_IndexOffset = 0
      m_IndexFactor = 0

      m_HashCode = 0

      m_StoredArrayLength = 0
   End Sub
#End Region

#Region "Index conversion methods"
   ''' <summary>
   ''' Gets the store index from the real index.
   ''' </summary>
   ''' <param name="realIndex">Real index.</param>
   ''' <returns>Store index.</returns>
   Private Function GetStoreIndex(realIndex As Integer) As Integer
      ' 2 is added so that the m_IndexOffset is not put out for a realIndex of 0
      ' and neither is the difference of m_IndexOffset and m_IndexFactor.
      Return (m_IndexFactor * (realIndex + 2)) - m_IndexOffset
   End Function

   ''' <summary>
   ''' Gets the real index from the store index.
   ''' </summary>
   ''' <param name="storeIndex">Store index.</param>
   ''' <returns>Real index.</returns>
   Private Function GetRealIndex(storeIndex As Integer) As Integer
      Return ((storeIndex + m_IndexOffset) \ m_IndexFactor) - 2
   End Function

   ''' <summary>
   ''' Gets the array index from the external index.
   ''' </summary>
   ''' <param name="externalIndex">External index.</param>
   ''' <returns>Index into the byte array.</returns>
   Private Function GetArrayIndex(externalIndex As Integer) As Integer
      Return GetRealIndex(m_IndexArray(externalIndex + m_IndexStart))
   End Function
#End Region

#Region "Data acess methods"
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

      Dim aByte As Byte

      Dim aHasher As SimpleHasher = New SimpleHasher(CLASS_HASH_SEED_1, CLASS_HASH_SEED_2)

      For i As Integer = 0 To count - 1
         aByte = sourceArray(sourceIndex)

         m_ByteArray(GetArrayIndex(i)) = aByte

         aHasher.UpdateHash(aByte)

         sourceIndex += 1
      Next

      m_HashCode = aHasher.GetHashValue()   ' Set hash code only once
   End Sub

   ''' <summary>
   ''' Gets the values from the shuffled array. 
   ''' </summary>
   ''' <returns>Values stored in the shuffled byte array.</returns>
   Private Function GetValues() As Byte()
      Dim result As Byte()

      result = New Byte(0 To Length - 1) {}

      For i As Integer = 0 To result.Length - 1
         result(i) = m_ByteArray(GetArrayIndex(i))
      Next

      Return result
   End Function
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

            If disposeManagedResources Then _
               ClearData()

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
         Return Not m_IsDisposed
      End Get
   End Property
#End Region
End Class
