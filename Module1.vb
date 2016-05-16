Imports System.IO

Module Module1

#Region "Declaraciones privadas"

    Private Const PATH_ENTRADA As String = "Entrada\"
    Private Const PATH_SALIDA As String = "Salida\"

#End Region

    Sub Main()

        Dim aux = My.Application.Info.DirectoryPath + "\" + PATH_ENTRADA

        Console.WriteLine("Carpeta entrada: " + aux)
        Console.WriteLine("Carpeta salida: " + My.Application.Info.DirectoryPath + "\" + PATH_SALIDA)

        Dim di As New DirectoryInfo(aux)

        ' procesa todos los xml's de la carpeta de entrada

        Dim ficherosxml() As FileInfo = di.GetFiles("*.xml")

        For Each fi As FileInfo In ficherosxml

            ProcesarXml(fi)

        Next

        Console.WriteLine("Ficheros procesados: " + ficherosxml.Count.ToString)
        Console.ReadLine()

    End Sub

    ''' <summary>
    ''' Extrae los mensajes contenidos en el fichero (incluye la ruta absoluta)
    ''' </summary>
    ''' <param name="fichero">Información del fichero a procesar</param>

    Private Sub ProcesarXml(ByVal fichero As FileInfo)

        Using FicheroEntrada As StreamReader = File.OpenText(fichero.FullName)

            Dim mensaje As New GestorMensajes(FicheroEntrada.ReadToEnd, PATH_SALIDA)
            Dim i As Byte

            FicheroEntrada.Close()

            Console.WriteLine("Fichero: " + fichero.Name + " Mensajes: " + CStr(mensaje.NumMensajes) +
                              " OK: " + CStr(mensaje.ListaMensajesOK.Count) +
                              " NO OK: " + CStr(mensaje.ListaErrores.Count))


            If mensaje.ListaErrores.Count > 0 Then

                Console.WriteLine("Problemas detectados:")

                For i = 0 To mensaje.ListaErrores.Count - 1

                    Console.WriteLine(mensaje.ListaErrores.Item(i))

                Next i

            End If

        End Using


    End Sub

End Module
