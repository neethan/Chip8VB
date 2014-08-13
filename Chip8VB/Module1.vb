' Chip8VB - Another Chip-8 Emulator; this time, it actually works!
' Copyright © Neethan Puvanendran 2014. All rights reserved.

Imports System.Drawing
Imports OpenTK
Imports OpenTK.Graphics
Imports OpenTK.Graphics.OpenGL
Imports OpenTK.Input
Imports Chip8VB.Chip8

Module Module1
    Dim WithEvents Window As New GameWindow(128, 64)

    Sub Main()
        Window.WindowBorder = WindowBorder.Fixed
        Window.Run(120, 60)     ' We'll make the Chip-8 do 120 cycles per second on average
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
    End Sub

    Private Sub Window_RenderFrame(sender As Object, e As OpenTK.FrameEventArgs) Handles Window.RenderFrame
        GL.Clear(ClearBufferMask.ColorBufferBit Or ClearBufferMask.DepthBufferBit)

        GL.MatrixMode(MatrixMode.Projection)
        GL.LoadIdentity()
        GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0)

        GL.Begin(BeginMode.Triangles)

        GL.Color3(Color.MidnightBlue)   ' Why not draw a triangle while waiting for me to finish the drawing code?
        GL.Vertex2(-1.0F, 1.0F)
        GL.Color3(Color.SpringGreen)
        GL.Vertex2(0.0F, -1.0F)
        GL.Color3(Color.Ivory)
        GL.Vertex2(1.0F, 1.0F)          ' I like triangles.

        GL.End()

        Window.SwapBuffers()
    End Sub

    Private Sub Window_UpdateFrame(sender As Object, e As OpenTK.FrameEventArgs) Handles Window.UpdateFrame
        If Window.Keyboard(Key.Escape) Then
            Window.Close()
        End If
    End Sub
End Module
