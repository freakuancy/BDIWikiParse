Public Class frmMessage
    Sub New(strText As String, strTitle As String)

        ' This call is required by the designer.
        InitializeComponent()
        TextBox1.Text = strText
        Me.Text = strTitle
        ' Add any initialization after the InitializeComponent() call.

    End Sub
    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        Me.Close()

    End Sub

    Private Sub frmMessage_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class