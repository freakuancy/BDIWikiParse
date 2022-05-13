Public Class frmOutput
    Public strOutput As String
    Public strName As String

    Public Sub New(_strOutput As String, _strName As String)

        ' This call is required by the designer.
        InitializeComponent()
        strOutput = _strOutput
        strName = _strName
        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Clipboard.SetText(TextBox1.Text)

    End Sub

    Private Sub frmOutput_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Generated Character Sheet (" + strName + ")"
        TextBox1.Text = strOutput
        If ParseTable.chkOnTop.Checked = True Then Me.TopMost = True
    End Sub

    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        Me.Close()

    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub
End Class