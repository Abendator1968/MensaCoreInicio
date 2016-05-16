Imports System.Configuration
Imports System.Data.Entity

Public Class MensajeriaDBContext
    Inherits DbContext

    Public Property Mensajes As DbSet(Of Mensaje)

    Public Sub New()

        MyBase.New(ConfigurationManager.ConnectionStrings("MensajeriaDBContext").ConnectionString)

    End Sub

    Shared Sub New()

        Database.SetInitializer(Of MensajeriaDBContext)(New MensajesBDInit())

    End Sub

    Public Class MensajesBDInit
        'Inherits NullDatabaseInitializer(Of MensajeriaDBContext)
        Inherits DropCreateDatabaseIfModelChanges(Of MensajeriaDBContext)

        Protected Overrides Sub Seed(ByVal context As MensajeriaDBContext)

            MyBase.Seed(context)

            CargaInicialBD(context)

        End Sub

        Private Sub CargaInicialBD(ByVal context As MensajeriaDBContext)

            'instrucciones para la carga inicial de la BD

        End Sub

    End Class

End Class
