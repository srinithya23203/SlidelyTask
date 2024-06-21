
Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json

Public Class ViewSubmissionsForm
    Private currentIndex As Integer = 0
    Private submissions As List(Of Submission)

    Private Async Sub ViewSubmissionsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        submissions = Await LoadSubmissionsAsync()
        DisplaySubmission(currentIndex)
    End Sub

    Private Sub DisplaySubmission(index As Integer)
        If index >= 0 And index < submissions.Count Then
            txtName.Text = submissions(index).Name
            txtEmail.Text = submissions(index).Email
            txtPhone.Text = submissions(index).Phone
            txtGithubLink.Text = submissions(index).GithubLink
            lblStopwatchTime.Text = submissions(index).StopwatchTime

            ' Disable fields by default
            txtName.ReadOnly = True
            txtEmail.ReadOnly = True
            txtPhone.ReadOnly = True
            txtGithubLink.ReadOnly = True
        End If
    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As EventArgs) Handles btnPrevious.Click
        If currentIndex > 0 Then
            currentIndex -= 1
            DisplaySubmission(currentIndex)
        End If
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        If currentIndex < submissions.Count - 1 Then
            currentIndex += 1
            DisplaySubmission(currentIndex)
        End If
    End Sub

    Private Async Function LoadSubmissionsAsync() As Task(Of List(Of Submission))
        Using client As New HttpClient()
            Dim response As HttpResponseMessage = Await client.GetAsync("http://localhost:3000/read")
            If response.IsSuccessStatusCode Then
                Dim jsonResponse As String = Await response.Content.ReadAsStringAsync()
                Return JsonConvert.DeserializeObject(Of List(Of Submission))(jsonResponse)
            Else
                MessageBox.Show("Error loading submissions.")
                Return New List(Of Submission)()
            End If
        End Using
    End Function

    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click
        If btnEdit.Text = "EDIT" Then
            ' Enable fields for editing
            txtName.ReadOnly = False
            txtEmail.ReadOnly = False
            txtPhone.ReadOnly = False
            txtGithubLink.ReadOnly = False

            ' Change the Edit button text to "SAVE"
            btnEdit.Text = "SAVE"
            btnEdit.BackColor = Color.LightBlue
        Else
            ' Save the updated submission
            SaveUpdatedSubmission()
        End If
    End Sub

    Private Async Sub SaveUpdatedSubmission()
        Dim submission As New Submission With {
            .Name = txtName.Text,
            .Email = txtEmail.Text,
            .Phone = txtPhone.Text,
            .GithubLink = txtGithubLink.Text,
            .StopwatchTime = lblStopwatchTime.Text
        }

        Dim json As String = JsonConvert.SerializeObject(submission)
        Using client As New HttpClient()
            Dim content As New StringContent(json, Encoding.UTF8, "application/json")
            Dim response As HttpResponseMessage = Await client.PutAsync($"http://localhost:3000/update?index={currentIndex}", content)
            If response.IsSuccessStatusCode Then
                submissions(currentIndex) = submission

                ' Disable fields after saving
                txtName.ReadOnly = True
                txtEmail.ReadOnly = True
                txtPhone.ReadOnly = True
                txtGithubLink.ReadOnly = True

                ' Reset the Edit button text
                btnEdit.Text = "EDIT"
                btnEdit.BackColor = Color.LightGreen

                MessageBox.Show("Update Successful!")
            Else
                MessageBox.Show("Error updating submission.")
            End If
        End Using
    End Sub

    Private Async Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        Using client As New HttpClient()
            Dim response As HttpResponseMessage = Await client.DeleteAsync($"http://localhost:3000/delete?index={currentIndex}")
            If response.IsSuccessStatusCode Then
                MessageBox.Show("Deletion Successful!")
                submissions.RemoveAt(currentIndex)
                If currentIndex >= submissions.Count Then
                    currentIndex = submissions.Count - 1
                End If
                DisplaySubmission(currentIndex)
            Else
                MessageBox.Show("Error deleting submission.")
            End If
        End Using
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = (Keys.Control Or Keys.P) Then
            btnPrevious.PerformClick()
            Return True
        ElseIf keyData = (Keys.Control Or Keys.N) Then
            btnNext.PerformClick()
            Return True
        ElseIf keyData = (Keys.Control Or Keys.E) Then
            btnEdit.PerformClick()
            Return True
        ElseIf keyData = (Keys.Control Or Keys.D) Then
            btnDelete.PerformClick()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function
End Class
