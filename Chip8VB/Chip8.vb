' Chip8VB - Another Chip-8 Emulator; this time, it actually works!
' Copyright © Neethan Puvanendran 2014. All rights reserved.
' 
'
' I suck at OOP, I'm so sorry.
'
' Chip-8 Keylayout
' 1 2 3 C
' 4 5 6 D
' 7 8 9 E
' A 0 B F
Public Class Chip8
    Private RAM(&HFFF) As Byte
    Private V(&HF) As Byte
    Private I As New UInt16
    Private PC As New UInt16
    Private SP As New Byte
    Private Stack(&HF) As UInt16
    Public Display(63, 31) As Integer
    Private ST As Byte
    Private DT As Byte
    Public Keys(&HF) As Boolean
    Public Repaint As Boolean = False
    Public Emulating As Boolean = False

    Public Sub New()

    End Sub

    Public Sub EmulateCycle()
        Dim Opcode As String = RAM(PC).ToString("X").PadLeft(2, "0"c) & RAM(PC + 1).ToString("X").PadLeft(2, "0"c)
        Dim X As Integer = Val("&H" & Opcode.Substring(1, 1))
        Dim Y As Integer = Val("&H" & Opcode.Substring(1, 1))
        Dim NN As Byte = Val("&H" & Opcode.Substring(2, 2))
        Dim ADDR As Integer = Val("&H" & Opcode.Substring(1, 3))


        If Opcode = "00E0" Then
            For a = 0 To 63
                For b = 0 To 31
                    Display(a, b) = 0
                Next
            Next
            Repaint = True
            PC += 2
        ElseIf Opcode = "00EE" Then
            SP -= 1
            PC = Stack(SP)
            PC += 2
        ElseIf Opcode.StartsWith("1") Then
            PC = ADDR
        ElseIf Opcode.StartsWith("2") Then
            Stack(SP) = PC
            SP += 1
            PC = ADDR
        ElseIf Opcode.StartsWith("3") Then
            If V(X) = NN Then
                PC += 4
            Else
                PC += 2
            End If
        ElseIf Opcode.StartsWith("4") Then
            If Not V(X) = NN Then
                PC += 4
            Else
                PC += 2
            End If
        ElseIf Opcode.StartsWith("5") Then
            If V(X) = V(Y) Then
                PC += 4
            Else
                PC += 2
            End If
        ElseIf Opcode.StartsWith("6") Then
            V(X) = NN
            PC += 2
        ElseIf Opcode.StartsWith("7") Then
            V(X) = V(X) + NN
            PC += 2
        ElseIf Opcode.StartsWith("8") Then
            If Opcode.EndsWith("0") Then
                V(X) = V(Y)
                PC += 2
            ElseIf Opcode.EndsWith("1") Then
                V(X) = V(X) Or V(Y)
                PC += 2
            ElseIf Opcode.EndsWith("2") Then
                V(X) = V(X) And V(Y)
                PC += 2
            ElseIf Opcode.EndsWith("3") Then
                V(X) = V(X) Xor V(Y)
                PC += 2
            ElseIf Opcode.EndsWith("4") Then
                If V(X) + V(Y) > 255 Then
                    V(&HF) = 1
                Else
                    V(&HF) = 0
                End If
                V(X) = V(X) + V(Y)
                PC += 2
            ElseIf Opcode.EndsWith("5") Then
                If V(X) > V(Y) Then
                    V(&HF) = 1
                Else
                    V(&HF) = 0
                End If
                V(X) = V(X) - V(Y)
                PC += 2

            ElseIf Opcode.EndsWith("6") Then
                Dim Binary As String = Convert.ToString(V(X), 2).PadLeft(8, "0"c)
                If Binary.EndsWith("1") Then
                    V(&HF) = 1
                Else
                    V(&HF) = 0
                End If
                V(X) = V(X) / 2
                PC += 2
            ElseIf Opcode.EndsWith("7") Then
                If V(Y) > V(X) Then
                    V(&HF) = 1
                Else
                    V(&HF) = 0
                End If
                V(X) = V(Y) - V(X)
                PC += 2
            ElseIf Opcode.EndsWith("E") Then
                Dim Binary As String = Convert.ToString(V(X), 2).PadLeft(8, "0"c)
                If Binary.StartsWith("1") Then
                    V(&HF) = 1
                Else
                    V(&HF) = 0
                End If
                V(X) = V(X) * 2
                PC += 2
            Else
                Emulating = False
                Throw New Exception(OpcodeError(Opcode))
            End If
        ElseIf Opcode.StartsWith("9") Then
            If Not V(X) = V(Y) Then
                PC += 4
            Else
                PC += 2
            End If
        ElseIf Opcode.StartsWith("A") Then
            I = ADDR
            PC += 2
        ElseIf Opcode.StartsWith("B") Then
            PC = ADDR + V(0)
        ElseIf Opcode.StartsWith("C") Then
            Dim RNG As New Random
            Dim Random As Byte = RNG.Next(0, 255)
            V(X) = NN And Random
            PC += 2
        ElseIf Opcode.StartsWith("D") Then
            ' i hate this opcode with a burning passion
            Dim nibble As Integer = Val("&H" & Opcode.Substring(3, 1))
            V(15) = 0
            Dim line As Byte
            For ypos = 0 To nibble
                Dim MemoryY = (V(Y) + ypos)
                line = RAM(I + ypos)
                For xpos = 0 To 7
                    Dim MemoryX = (V(X) + xpos)
                    Dim b As String = Convert.ToString(line, 2).PadLeft(8, "0"c).Substring(xpos, 1)
                    If b = "1" Then
                        If (Display(MemoryX, MemoryY) Xor 1) = 0 Then
                            V(15) = 1
                        End If
                    End If
                Next
            Next
            Repaint = True
            PC += 2
        ElseIf Opcode.StartsWith("E") Then
            If Opcode.EndsWith("9E") Then
                If Keys(V(X)) Then
                    PC += 4
                Else
                    PC += 2
                End If
            ElseIf Opcode.EndsWith("A1") Then
                If Not Keys(V(X)) Then
                    PC += 4
                Else
                    PC += 2
                End If
            Else
                Emulating = False
                Throw New Exception(OpcodeError(Opcode))
            End If
        ElseIf Opcode.StartsWith("F") Then
            If Opcode.EndsWith("07") Then
                V(X) = DT
                PC += 2
            ElseIf Opcode.EndsWith("0A") Then
                If Console.KeyAvailable Then    ' Non-blocking, so execution can continue peacefully (yay)
                    Dim Key As ConsoleKeyInfo = Console.ReadKey(False)
                    Dim KeyWasPressed As Boolean = True
                    Select Case Key.Key
                        Case ConsoleKey.D1
                            V(X) = 1
                        Case ConsoleKey.D2
                            V(X) = 2
                        Case ConsoleKey.D3
                            V(X) = 3
                        Case ConsoleKey.D4
                            V(X) = &HC

                        Case ConsoleKey.Q
                            V(X) = 4
                        Case ConsoleKey.W
                            V(X) = 5
                        Case ConsoleKey.E
                            V(X) = 6
                        Case ConsoleKey.R
                            V(X) = &HD

                        Case ConsoleKey.A
                            V(X) = 7
                        Case ConsoleKey.S
                            V(X) = 8
                        Case ConsoleKey.D
                            V(X) = 9
                        Case ConsoleKey.F
                            V(X) = &HE

                        Case ConsoleKey.Z
                            V(X) = &HA
                        Case ConsoleKey.X
                            V(X) = 0
                        Case ConsoleKey.C
                            V(X) = &HB
                        Case ConsoleKey.V
                            V(X) = &HF
                        Case Else
                            KeyWasPressed = False
                    End Select
                    If KeyWasPressed = True Then
                        PC += 2
                    End If
                End If
            ElseIf Opcode.EndsWith("15") Then
                DT = V(X)
                PC += 2
            ElseIf Opcode.EndsWith("18") Then
                ST = V(X)
                PC += 2
            ElseIf Opcode.EndsWith("1E") Then
                I += V(X)
                PC += 2
            ElseIf Opcode.EndsWith("29") Then
                Select Case V(X)
                    Case 0
                        I = 0
                    Case 1
                        I = 5
                    Case 2
                        I = 10
                    Case 3
                        I = 15
                    Case 4
                        I = 20
                    Case 5
                        I = 25
                    Case 6
                        I = 30
                    Case 7
                        I = 35
                    Case 8
                        I = 40
                    Case 9
                        I = 45
                    Case &HA
                        I = 50
                    Case &HB
                        I = 55
                    Case &HC
                        I = 60
                    Case &HD
                        I = 65
                    Case &HE
                        I = 70
                    Case &HF
                        I = 75
                End Select
                PC += 2
            ElseIf Opcode.EndsWith("33") Then
                RAM(I) = V(X) / 100
                RAM(I + 1) = (V(X) / 10) Mod 10
                RAM(I + 2) = (V(X) Mod 100) Mod 10
                PC += 2
            ElseIf Opcode.EndsWith("55") Then
                For a = 0 To X
                    RAM(I + a) = V(X)
                Next
                PC += 2
            ElseIf Opcode.EndsWith("65") Then
                For a = 0 To X
                    V(X) = RAM(I + a)
                Next
                PC += 2
            Else
                Emulating = False
                Throw New Exception(OpcodeError(Opcode))
            End If
        End If

    End Sub

    Public Sub New(ByVal Rom As String)
        If Rom = Nothing Or Rom = String.Empty Then
            Throw New Exception("No ROM loaded!")
        End If
        PC = &H200
        I = 0
        SP = 0
        ST = 0
        DT = 0
        For a = 0 To 63
            For b = 0 To 31
                Display(a, b) = 0
            Next
        Next
        Repaint = True
        For a = 0 To &HF
            Stack(a) = 0
            V(a) = 0
            Keys(a) = False
        Next
        Dim characters As Byte()
        characters = {
                        &HF0, &H90, &H90, &H90, &HF0,
                        &H20, &H60, &H20, &H20, &H70,
                        &HF0, &H10, &HF0, &H80, &HF0,
                        &HF0, &H10, &HF0, &H10, &HF0,
                        &H90, &H90, &HF0, &H10, &H10,
                        &HF0, &H80, &HF0, &H10, &HF0,
                        &HF0, &H80, &HF0, &H90, &HF0,
                        &HF0, &H10, &H20, &H40, &H40,
                        &HF0, &H90, &HF0, &H90, &HF0,
                        &HF0, &H90, &HF0, &H10, &HF0,
                        &HF0, &H90, &HF0, &H90, &H90,
                        &HE0, &H90, &HE0, &H90, &HE0,
                        &HF0, &H80, &H80, &H80, &HF0,
                        &HE0, &H90, &H90, &H90, &HE0,
                        &HF0, &H80, &HF0, &H80, &HF0,
                        &HF0, &H80, &HF0, &H80, &H80
                    }
        For a = 0 To 79
            RAM(a) = characters(a)
        Next
    End Sub

    Private Function OpcodeError(ByVal opcode As String)
        Return ("Invalid opcode: " & opcode _
                                    & vbNewLine & "PC: " & PC _
                                    & vbNewLine & "SP: " & SP _
                                    & vbNewLine & "Stack(" & SP & "): " & Stack(SP) _
                                    & vbNewLine & "I: " & I _
                                    & vbNewLine & "V(0): " & V(0) _
                                    & vbNewLine & "V(1): " & V(1) _
                                    & vbNewLine & "V(2): " & V(2) _
                                    & vbNewLine & "V(3): " & V(3) _
                                    & vbNewLine & "V(4): " & V(4) _
                                    & vbNewLine & "V(5): " & V(5) _
                                    & vbNewLine & "V(6): " & V(6) _
                                    & vbNewLine & "V(7): " & V(7) _
                                    & vbNewLine & "V(8): " & V(8) _
                                    & vbNewLine & "V(9): " & V(9) _
                                    & vbNewLine & "V(A): " & V(&HA) _
                                    & vbNewLine & "V(B): " & V(&HB) _
                                    & vbNewLine & "V(C): " & V(&HC) _
                                    & vbNewLine & "V(D): " & V(&HD) _
                                    & vbNewLine & "V(E): " & V(&HE) _
                                    & vbNewLine & "V(F): " & V(&HF))
    End Function
End Class
