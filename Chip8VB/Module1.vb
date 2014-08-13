' Chip8VB - Another Chip-8 Emulator; this time, it actually works!
' Copyright © Neethan Puvanendran 2014. All rights reserved.

Imports System.Drawing
Imports OpenTK
Imports OpenTK.Graphics
Imports OpenTK.Graphics.OpenGL
Imports OpenTK.Input
Imports Chip8VB.Chip8

Module Module1
    Dim WithEvents Window As GameWindow
    Dim Chip8vb As Chip8
    Dim WindowMultiplier As Integer = 2

    Sub Main(args() As String)
        Chip8vb = New Chip8(args(0).ToString)
        Window = New GameWindow(128 * WindowMultiplier, 64 * WindowMultiplier)
        Window.WindowBorder = WindowBorder.Fixed
        Window.Run(180, 60)     ' We'll make the Chip-8 do 120 cycles per second on average
        Console.WriteLine("Emulation halted! Choose a new game? (Y/N)")
        Dim key As ConsoleKeyInfo = Console.ReadKey
        If key.Key = ConsoleKey.N Then
            Window.Dispose()
            End
        End If
    End Sub

    Private Sub Window_Closing(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles Window.Closing

    End Sub

    Private Sub Window_Load(sender As Object, e As System.EventArgs) Handles Window.Load
        Window.VSync = VSyncMode.On
        Dim w = Window.Width                ' Width and height of the gamewindow
        Dim h = Window.Height
        GL.MatrixMode(MatrixMode.Projection)
        GL.LoadIdentity()
        GL.Ortho(0, w, h, 0, 0, 1)
        GL.MatrixMode(MatrixMode.Modelview)
        GL.Translate(0.375, 0.375, 0)
        GL.Disable(EnableCap.DepthTest)     ' Drawing in 2D
    End Sub

    Private Sub Window_RenderFrame(sender As Object, e As OpenTK.FrameEventArgs) Handles Window.RenderFrame
        Dim w = Window.Width
        Dim h = Window.Height
        GL.MatrixMode(MatrixMode.Modelview)
        GL.LoadIdentity()

        If Chip8vb.Repaint Then
            For y = 0 To 31
                For x = 0 To 63
                    If Chip8vb.Display(x, y) = 1 Then
                        GL.Color3(Color.White)
                        GL.Begin(BeginMode.Points)
                        GL.Vertex2(x, y)
                        GL.End()
                    ElseIf Chip8vb.Display(x, y) = 0 Then
                        GL.Color3(Color.Black)
                        GL.Begin(BeginMode.Points)
                        GL.Vertex2(x, y)
                        GL.End()
                    End If
                Next
            Next
            Chip8vb.Repaint = False
        End If

        Window.SwapBuffers()
    End Sub

    Private Sub Window_UpdateFrame(sender As Object, e As OpenTK.FrameEventArgs) Handles Window.UpdateFrame
        If Window.Keyboard(Key.Escape) Then
            Window.Close()
        End If
        Chip8vb.EmulateCycle()
        If Chip8vb.DT > 0 Then
            Chip8vb.DT -= 1
        End If
        If Chip8vb.ST > 0 Then
            Chip8vb.ST -= 1
        End If
    End Sub
End Module
