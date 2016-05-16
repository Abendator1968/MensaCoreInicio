Imports System.Xml
Imports MensajesCore.Mensaje
Imports System.Data.Entity.Infrastructure
Imports System.Text

Public Class GestorMensajes

#Region "Declaraciones públicas"

#End Region

#Region "Declaraciones privadas"

    Private RutaSalidaOK As String
    Private RutaSalidaNOK As String

    Private Enum TipoMensaje As Byte

        Envio = 0
        Aceptacion = 1
        Rechazo = 2
        Activacion = 3
        SolicitudAnulacion = 4
        AceptacionAnulacion = 5
        RechazoAnulacion = 6
        NoRealizacion = 7
        SolicitudInfoAdicional = 8
        EnvioInfoAdicional = 9
        Retipificacion = 10

    End Enum

    Private Enum TipoFecha As Byte

        FEnvio = 0
        FAceptacion = 1
        FRechazo = 2
        FAnulacion = 3
        FActivacion = 4
        FNoRealizacion = 5
        FPeticionInfoAdicional = 6
        FRetipificacion = 7
        FEnvioInfoAdicional = 8

    End Enum

    Private DiccionarioFechas As New Dictionary(Of TipoFecha, String())

    Private DiccionarioEtiquetas As New Dictionary(Of String, String())

#End Region

#Region "Propiedades"

    Public Property CupsId As String
    Public Property ProcesoId() As String
    Public Property PasoId() As String
    Public Property LineaId As LineaNegocio
    Public Property CodSolicitud As String
    Public Property EmpresaOrigen As String
    Public Property EmpresaDestino As String
    Public Property NumMensajes As Integer
    Public Property ListaMensajesOK As New List(Of Mensaje)
    Public Property ListaMensajesNOK As New List(Of Mensaje)
    Public Property ListaErrores As New List(Of String)

#End Region

#Region "Métodos privados"

    ''' <summary>
    ''' Determina el valor de varias propiedades a partir del XML recibido en la entrada
    ''' </summary>
    ''' <param name="xml">XML que será parseado para determinar el valor de varias propiedades</param>

    Private Sub ObtPropiedades(ByVal xml As XmlDocument)

        Dim listanodos As XmlNodeList

        'Determinación de la línea de negocio: buscamos una etiqueta propia de gas. Si no se obtiene ningún nodo, el xml es de luz

        listanodos = xml.GetElementsByTagName("heading")

        If listanodos.Count = 0 Then

            LineaId = LineaNegocio.Electricidad

        Else

            LineaId = LineaNegocio.Gas

        End If

        'Determinación del Cups (y del número de mensajes que contiene el xml recibido a partir del número de cups obtenido)

        CupsId = BuscaEtiqueta("CUPS", xml)

        'Determinación del proceso

        ProcesoId = BuscaEtiqueta("CodProceso", xml)

        'Determinación del paso

        PasoId = BuscaEtiqueta("CodPaso", xml)

        'Determinación de la empresa emisora

        EmpresaOrigen = BuscaEtiqueta("EmpEmisora", xml)

        'Determinación de la empresa destino

        EmpresaDestino = BuscaEtiqueta("EmpDestino", xml)

    End Sub

    ''' <summary>
    ''' Trocea un XML en varios que comparten cabecera, en base a una etiqueta
    ''' </summary>
    ''' <param name="xml">XML a trozear</param>
    ''' <param name="eticab">etiqueta usada para determinar la cabecera común a utilizar</param>
    ''' <param name="eticuerpo">etiqueta usada para determinar los distintos nodos que se combinarán con una única cabecera</param>

    Private Sub TroceaXML(ByVal xml As XmlDocument, ByVal eticab As String, ByVal eticuerpo As String)

        Dim cabecera As String = xml.GetElementsByTagName(eticab).Item(0).OuterXml

        ' obtiene la primera línea del documento xml

        Dim linea1 As String = xml.GetElementsByTagName(eticab).Item(0).OwnerDocument.FirstChild.OuterXml

        ' obtiene el nodo raíz del documento (del que colgará el elemento cabecera y cada uno de los n elementos "cuerpo"
        ' que se calculan en el bucle de más abajo) (<sctdapplication>(*AQUÍ*)/sctdapplication>). Justo en medio (*AQUÍ*)
        ' hay que insertar la cabecera con cada uno de los "cuerpos"

        Dim aux2 As String = xml.GetElementsByTagName(eticab).Item(0).ParentNode.OuterXml 'elemento <sctdapplication>
        Dim aux3 As String = xml.GetElementsByTagName(eticab).Item(0).ParentNode.InnerXml 'contenido del elemento <sctdapplication>

        Dim listanodos As XmlNodeList = xml.GetElementsByTagName(eticuerpo)

        For Each nodo In listanodos

            Dim cuerpo As String

            cuerpo = nodo.ParentNode.OuterXml ' en gas obtenemos el cuerpo correspodiente al nodo actual (hay más de uno)

            ' aux4 es el resultado de concatenar la primera línea con el resultado de sustituir el contenido del elemento
            ' <sctdapplication> por la concatenación de la cabecera (común) y el cuerpo "n" obtenido en cada iteración

            Dim aux4 As String = linea1 + aux2.Replace(aux3, cabecera + cuerpo)

            ProcesaXML(aux4)

        Next

    End Sub

    ''' <summary>
    ''' Procesa un XML individual de gas que recibe como parámetro
    ''' </summary>
    ''' <param name="xml">Cadena que representa al xml a procesar</param>

    Private Sub ProcesaXML(ByVal xml As String)

        Dim docxml As New XmlDocument

        docxml.LoadXml(xml)

        ' determinación del código de solicitud ATR

        CodSolicitud = BuscaEtiqueta("CodSolicitud", docxml)

        Dim mensaje As Mensaje = GuardaMensajeEnBD(docxml)

        If mensaje.ErrorMensaje = String.Empty Then

            ListaMensajesOK.Add(mensaje)

        Else

            ListaMensajesNOK.Add(mensaje)

        End If

        Me.NumMensajes += 1

    End Sub

    ''' <summary>
    ''' Genera una instancia de mensaje, la salva en la BD. Devuelve el objeto instanciado si todo va bien y Nothing en caso contrario
    ''' </summary>
    ''' <param name="docxml">XML a trozear</param>

    Private Function GuardaMensajeEnBD(ByVal docxml As XmlDocument) As Mensaje

        Dim Cups As String = BuscaEtiqueta("CUPS", docxml)

        Dim fecha_aux = FechaEfecto(docxml.InnerXml)

        Dim mensaje As New Mensaje With {.CupsId = Cups, .ProcesoId = Me.ProcesoId, .PasoId = Me.PasoId,
                                        .LineaId = Me.LineaId, .Estado = EstadoMensaje.Pendiente, .FechaCargaBD = Now,
                                        .CodSolicitud = Me.CodSolicitud,
                                        .EmpresaOrigen = Me.EmpresaOrigen, .EmpresaDestino = Me.EmpresaDestino, .Xml = docxml.InnerXml,
                                        .RutaXml = RutaSalidaOK + Cups + "-" + Format(Now, "yyyyMMdd") + ".xml", .Timestamp = Now}

        'FechaEfecto sólo se informa si el valor es distinto de nulo (representado por MinValue). En cualquier otro caso no se informa para que tome valor nulo en la BD

        If fecha_aux <> Date.MinValue Then

            mensaje.FechaEfecto = fecha_aux

        End If

        Using db As New MensajeriaDBContext()

            'Dim utcNow As DateTime = DateTime.UtcNow
            'Dim utcNowAsLong As Long = utcNow.ToBinary()
            'Dim utcNowBytes As Byte() = BitConverter.GetBytes(utcNowAsLong)
            Dim ErrorMensaje As String = String.Empty

            Try

                db.Mensajes.Add(mensaje)

                Try

                    db.SaveChanges()

                Catch ex As System.Data.SqlClient.SqlException

                    ErrorMensaje = "CUPS: " + Cups + ". Error de BD:  " + ex.Message
                    Me.ListaErrores.Add(ErrorMensaje)

                Catch ex As DbUpdateConcurrencyException

                    ErrorMensaje = "CUPS: " + Cups + ". Error de concurrencia: " + ex.Message
                    Me.ListaErrores.Add(ErrorMensaje)

                Catch ex As Entity.Validation.DbEntityValidationException

                    Dim DetalleError As String = ex.EntityValidationErrors(0).ValidationErrors(0).ErrorMessage
                    Dim PropiedadError As String = ex.EntityValidationErrors(0).ValidationErrors(0).PropertyName

                    ErrorMensaje = "CUPS: " + Cups + " Propiedad: " + PropiedadError + " Error: " + DetalleError
                    Me.ListaErrores.Add(ErrorMensaje)

                Catch ex As Exception

                    ErrorMensaje = "CUPS: " + Cups + " " + ex.InnerException.InnerException.Message
                    Me.ListaErrores.Add(ErrorMensaje)

                End Try

            Catch ex As System.Data.SqlClient.SqlException

                ErrorMensaje = "CUPS: " + Cups + ". Error de BD:  " + ex.Message
                Me.ListaErrores.Add(ErrorMensaje) 'PDTE: hacer que finalice el proceso pues se trata de un error grave (por ej. no se puede hacer el drop-create table)

            Catch ex As Exception

                ErrorMensaje = "CUPS: " + Cups + " " + ex.InnerException.InnerException.Message
                Me.ListaErrores.Add(ErrorMensaje)

            End Try

            ' si ha existido error, guarda el detalle del mismo y genera el path a la carpeta de xml's NOK

            If ErrorMensaje IsNot String.Empty Then

                mensaje.ErrorMensaje = ErrorMensaje
                mensaje.RutaXml = RutaSalidaNOK + Cups + "-" + Format(Now, "yyyyMMdd") + ".xml"

            End If

        End Using

        Return mensaje

    End Function

    Private Function ObtTipoMensaje() As TipoMensaje

        Dim rtdo As TipoMensaje

        Select Case LineaId

            Case LineaNegocio.Electricidad

                If ProcesoId = "D1" Then 'PDTE: Detectar todas las respuestas unilaterales de electricidad

                    rtdo = TipoMensaje.Activacion

                Else

                    Select Case PasoId

                        Case "01"

                            rtdo = TipoMensaje.Envio

                        Case "02"

                            rtdo = TipoMensaje.Aceptacion

                        Case "05"

                            rtdo = TipoMensaje.Activacion

                    End Select

                End If

            Case LineaNegocio.Gas

                Select Case PasoId

                    Case "A1"

                        rtdo = TipoMensaje.Envio

                    Case "A2"

                        rtdo = TipoMensaje.Aceptacion

                    Case "A3"

                        rtdo = TipoMensaje.Activacion

                End Select

        End Select

        Return rtdo

    End Function

    Private Sub CargaDiccionario()

        ' conceptos generales

        DiccionarioEtiquetas.Add("CUPS", {"Codigo", "cups", "CUPS"})
        DiccionarioEtiquetas.Add("CodSolicitud", {"CodigoDeSolicitud", "comreferencenum"}) ' cod solicitud comercializadora
        DiccionarioEtiquetas.Add("CodProceso", {"CodigoDelProceso", "processcode"})
        DiccionarioEtiquetas.Add("CodPaso", {"CodigoDePaso", "messagetype"})
        DiccionarioEtiquetas.Add("EmpEmisora", {"CodigoREEEmpresaEmisora", "dispatchingcompany"})
        DiccionarioEtiquetas.Add("EmpDestino", {"CodigoREEEmpresaDestino", "destinycompany"})

        ' fechas de envío

        DiccionarioFechas.Add(TipoFecha.FEnvio, {"FechaSolicitud"})

        ' fechas de aceptación

        DiccionarioFechas.Add(TipoFecha.FAceptacion, {"responsedate", "FechaAceptacion"})

        ' fechas de activación

        DiccionarioFechas.Add(TipoFecha.FActivacion, {"Fecha", "transfereffectivedate"})

    End Sub

    Private Function FechaEfecto(ByVal doc As String) As Date

        Dim aux As String = String.Empty
        Dim DocXml As New XmlDocument

        DocXml.LoadXml(doc)

        Select Case ObtTipoMensaje()

            Case TipoMensaje.Envio

                aux = BuscaFecha(TipoFecha.FEnvio, DocXml)

            Case TipoMensaje.Aceptacion

                aux = BuscaFecha(TipoFecha.FAceptacion, DocXml)

            Case TipoMensaje.Activacion

                aux = BuscaFecha(TipoFecha.FActivacion, DocXml)

        End Select

        If aux IsNot String.Empty Then

            Return New Date(CInt(aux.Substring(0, 4)), CInt(aux.Substring(5, 2)), CInt(aux.Substring(8, 2)))

        Else

            Return Date.MinValue

        End If

    End Function

    Private Function BuscaFecha(ByVal tipofecha As TipoFecha, ByVal DocXml As XmlDocument) As String

        Dim listanodos As XmlNodeList
        Dim aux As String = String.Empty

        For Each valor As String In DiccionarioFechas(tipofecha)

            listanodos = DocXml.GetElementsByTagName(valor)

            If listanodos.Count = 1 Then

                aux = listanodos.Item(0).InnerText

            End If

            If aux IsNot String.Empty Then

                Exit For

            End If

        Next

        Return aux

    End Function

    Private Function BuscaEtiqueta(ByVal etiqueta As String, ByVal DocXml As XmlDocument) As String

        Dim listanodos As XmlNodeList
        Dim aux As String = String.Empty

        For Each valor As String In DiccionarioEtiquetas(etiqueta)

            listanodos = DocXml.GetElementsByTagName(valor)

            Select Case listanodos.Count

                Case 0

                    Continue For

                Case 1

                    aux = listanodos.Item(0).InnerText
                    Exit For

                Case Else

                    aux = "más de 1"
                    Exit For

            End Select

        Next

        Return aux

    End Function

#End Region

#Region "Métodos públicos"

    Public Sub New(ByVal doc As String, ByVal RutaSalida As String)

        Dim docxmlEntrada As New XmlDocument

        ' construye la ruta de salida de los XML's (OK's y no OK's)

        RutaSalidaOK = RutaSalida + "OK\"
        RutaSalidaNOK = RutaSalida + "NOK\"

        ' carga el diccionario de atributos (etiquetas que aluden al mismo concepto en distintos mensajes)

        CargaDiccionario()

        ' convierte la cadena de texto a xml

        docxmlEntrada.LoadXml(doc)

        ' obtiene de forma automática el valor de varias propiedades a partir del xml recibido

        ObtPropiedades(docxmlEntrada)

        ' obtiene los mensajes individuales contenidos en el xml de entrada (sólo ocurre en el caso de gas, de ahí que se usen
        ' etiquetas propias de gas: "heading" y "cups"

        If LineaId = LineaNegocio.Gas Then

            TroceaXML(docxmlEntrada, "heading", "cups")

        Else

            ProcesaXML(docxmlEntrada.InnerXml)

        End If

        ' Salva a disco los mensajes OK y luego los no OK

        If Me.ListaMensajesOK.Count > 0 Then

            For Each mensa As Mensaje In Me.ListaMensajesOK

                Dim wr As New XmlTextWriter(mensa.RutaXml, Encoding.GetEncoding("ISO-8859-1"))
                Dim docxmlOK As New XmlDocument

                docxmlOK.LoadXml(mensa.Xml)

                wr.Formatting = Formatting.None
                docxmlOK.Save(wr)
                wr.Close()

            Next

        End If

        If Me.ListaMensajesNOK.Count > 0 Then

            For Each mensa As Mensaje In Me.ListaMensajesNOK

                Dim wr As New XmlTextWriter(mensa.RutaXml, Encoding.GetEncoding("ISO-8859-1"))
                Dim docxmlNOK As New XmlDocument

                docxmlNOK.LoadXml(mensa.Xml)

                wr.Formatting = Formatting.None
                docxmlNOK.Save(wr)
                wr.Close()

            Next

        End If

    End Sub

#End Region

End Class
