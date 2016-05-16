Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Public Class Mensaje

#Region "Declaraciones públicas"

    Public Enum EstadoMensaje As Byte

        Pendiente = 0
        ConErrorDeImportacion = 1
        ConErrorTrasCarga = 2
        PdteVolcarASistemas = 3
        Finalizado = 4

    End Enum

    Public Enum LineaNegocio As Byte

        Electricidad = 1
        Gas = 2

    End Enum

#End Region

#Region "Propiedades"

    <Key(), Column(Order:=1)>
    <Index("Ind_Mensaje", IsUnique:=True, Order:=1)>
    <MinLength(20, ErrorMessage:="CUPS con tamaño inferior al permitido"), MaxLength(22, ErrorMessage:="CUPS con tamaño superior al permitido")>
    <Required(ErrorMessage:="Campo obligatorio")>
    Public Property CupsId As String

    <Key(), Column(Order:=2)>
    <Index("Ind_Mensaje", IsUnique:=True, Order:=2)>
    <MinLength(2, ErrorMessage:="La longitud debe ser 2"), MaxLength(2, ErrorMessage:="La longitud debe ser 2")>
    <Required(ErrorMessage:="Campo obligatorio")>
    Public Property ProcesoId() As String

    <Key(), Column(Order:=3)>
    <Index("Ind_Mensaje", IsUnique:=True, Order:=3)>
    <MinLength(2, ErrorMessage:="La longitud debe ser 2"), MaxLength(2, ErrorMessage:="La longitud debe ser 2")>
    <Required(ErrorMessage:="Campo obligatorio")>
    Public Property PasoId() As String

    <Required(ErrorMessage:="Campo obligatorio")>
    Public Property LineaId As LineaNegocio

    <Required(ErrorMessage:="Campo obligatorio")>
    Public Property Estado As EstadoMensaje

    ' FechaEfecto es la fecha de envío, aceptación, rechazo, activación etc ... en función del tipo de respuesta
    ' el signo ? junto al tipo indica que se admite el valor nulo (es lo contrario a poner <required>)
    <DisplayFormat(DataFormatString:="dd/MM/yyyy")>
    Public Property FechaEfecto As Date?

    <DisplayFormat(DataFormatString:="dd/MM/yyyy")>
    <Required(ErrorMessage:="Campo obligatorio")>
    Public Property FechaCargaBD As Date

    '<Required(ErrorMessage:="Campo obligatorio")> ' en BCC's de gas este dato no llega informado. Lo dejamos en blanco para distinguir BCC's de ACC's
    <Key(), Column(Order:=4)>
    <Index("Ind_Mensaje", IsUnique:=True, Order:=4)>
    Public Property CodSolicitud As String

    <MinLength(4, ErrorMessage:="La longitud debe ser 4"), MaxLength(4, ErrorMessage:="La longitud debe ser 4")>
    <Required(ErrorMessage:="Campo obligatorio")>
    Public Property EmpresaOrigen As String

    <MinLength(4, ErrorMessage:="La longitud debe ser 4"), MaxLength(4, ErrorMessage:="La longitud debe ser 4")>
    <Required(ErrorMessage:="Campo obligatorio")>
    Public Property EmpresaDestino As String

    <Required(ErrorMessage:="Campo obligatorio")>
    Public Property RutaXml As String

    '<DatabaseGenerated(DatabaseGeneratedOption.Computed)>
    <Required(ErrorMessage:="Campo obligatorio")>
    Public Property Timestamp As Date

    <Timestamp>
    Public Property VersionRegistro As Byte() ' el atributo <Timestamp> exige que la propiedad sea del tipo Byte()

    ' Propiedades que no se mapean a tabla

    <NotMapped>
    Public Property ErrorMensaje As String

    <NotMapped>
    Public Property Xml As String

#End Region

#Region "Métodos públicos"

#End Region

End Class
