Public Class OracleQuickQuery
   Private _schemaName As String
   ''' <summary>
   ''' The schema to query the data from
   ''' </summary>
   ''' <remarks></remarks>
   Public Property SchemaName() As String
      Get
         Return _schemaName
      End Get
      Set(ByVal value As String)
         _schemaName = value
      End Set
   End Property

   Private _tableName As String
   ''' <summary>
   ''' The name of the table to retrieve the data from
   ''' </summary>
   ''' <remarks></remarks>
   Public Property TableName() As String
      Get
         Return _tableName
      End Get
      Set(ByVal value As String)
         _tableName = value
      End Set
   End Property

   Private _fieldName As String
   ''' <summary>
   ''' The name of the field to query from the table. Field names like UPPER(FIELD) are valid.
   ''' </summary>
   ''' <value></value>
   ''' <returns></returns>
   ''' <remarks></remarks>
   Public Property FieldName() As String
      Get
         Return _fieldName
      End Get
      Set(ByVal value As String)
         _fieldName = value
      End Set
   End Property

   Private _fieldAlias As String
   ''' <summary>
   ''' The name to apply to the field in the return dataset
   ''' </summary>
   ''' <remarks></remarks>
   Public Property FieldAlias() As String
      Get
         Return _fieldAlias
      End Get
      Set(ByVal value As String)
         _fieldAlias = value
      End Set
   End Property

   Private _whereClause As String
   ''' <summary>
   ''' The WHERE clause of the query without the WHERE keyword. Use {PARAMETER} as a placeholder for the ParameterValue.
   ''' </summary>
   ''' <remarks><example><code>LOCATION_ID={PARAMETER}</code></example></remarks>
   Public Property WhereClause() As String
      Get
         Return _whereClause
      End Get
      Set(ByVal value As String)
         _whereClause = value
      End Set
   End Property

   Private _parameterValue As String
   ''' <summary>
   ''' The value to replace the {PARAMETER} placeholder with when running the query.
   ''' </summary>
   ''' <remarks></remarks>
   Public Property ParameterValue() As String
      Get
         Return _parameterValue
      End Get
      Set(ByVal value As String)
         _parameterValue = value
      End Set
   End Property

   Private _result As String
   ''' <summary>
   ''' The result from executing the query
   ''' </summary>
   ''' <remarks></remarks>
   Public Property Result() As String
      Get
         Return _result
      End Get
      Set(ByVal value As String)
         _result = value
      End Set
   End Property

   ''' <summary>
   ''' Default constructor.
   ''' </summary>
   ''' <remarks></remarks>
   Public Sub New()

   End Sub

   ''' <summary>
   ''' Construct a new OracleQuickQuery with the specified values. The query will be of the form
   ''' SELECT fieldName AS fieldAlias FROM schemaName.tableName WHERE whereClause
   ''' </summary>
   ''' <param name="schemaName">The name of the schema to query.</param>
   ''' <param name="tableName">The name of the table to query.</param>
   ''' <param name="fieldName">The name of the field to query. Field names such as UPPER(FIELD) are valid.</param>
   ''' <param name="fieldAlias">The name to apply to the field in the return dataset.</param>
   ''' <param name="whereClause">The where portion of the query, without the WHERE keyword. Use {PARAMETER} as 
   ''' a placeholder for the parameterValue.</param>
   ''' <remarks></remarks>
   Public Sub New(ByVal schemaName As String, ByVal tableName As String, ByVal fieldName As String, _
                  ByVal fieldAlias As String, ByVal whereClause As String)
      Me.SchemaName = schemaName
      Me.TableName = tableName
      Me.FieldName = fieldName
      Me.FieldAlias = fieldAlias
      Me.WhereClause = whereClause
   End Sub

   ''' <summary>
   ''' Returns the SQL representation of the query
   ''' </summary>
   ''' <remarks></remarks>
   Public Overrides Function ToString() As String
      Dim sqlStatement As String = "SELECT " & FieldName & " AS " & FieldAlias _
            & " FROM " & SchemaName & "." & TableName _
            & " WHERE " & WhereClause.Replace("{PARAMETER}", ParameterValue)
      Return sqlStatement
   End Function
End Class
