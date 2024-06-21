Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json

Public Class ViewSubmissionForm

    Private ReadOnly client As New HttpClient()
    Private submissions As List(Of Submission2) = New List(Of Submission2)()
    Private currentIndex As Integer = 0

    Private Async Sub ViewSubmissionForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Await LoadSubmissions()
        DisplaySubmission(currentIndex)
        btnSaveChanges.Visible = False
    End Sub

    Private Sub ViewSubmissionForm_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.Control AndAlso e.KeyCode = Keys.P Then
            btnPrevious.PerformClick()
        ElseIf e.Control AndAlso e.KeyCode = Keys.N Then
            btnNext.PerformClick()
        ElseIf e.Control AndAlso e.KeyCode = Keys.D Then
            btnDelete.PerformClick()
        ElseIf e.Control AndAlso e.KeyCode = Keys.E Then
            btnEdit.PerformClick()
        End If
    End Sub
    Private Async Function LoadSubmissions() As Task
        Try
            Dim response As HttpResponseMessage = Await client.GetAsync("http://localhost:3000/read")
            If response.IsSuccessStatusCode Then
                Dim json As String = Await response.Content.ReadAsStringAsync()
                submissions = JsonConvert.DeserializeObject(Of List(Of Submission2))(json)
            Else
                MessageBox.Show("Failed to retrieve submissions from the server.")
            End If
        Catch ex As Exception
            MessageBox.Show($"Error occurred while retrieving submissions: {ex.Message}")
        End Try
    End Function

    Private Sub DisplaySubmission(index As Integer)
        If index >= 0 AndAlso index < submissions.Count Then
            Dim submission As Submission2 = submissions(index)
            txtName.Text = submission.Name
            txtEmail.Text = submission.Email
            txtPhone.Text = submission.Phone
            txtGithub.Text = submission.GitHubLink
            lblStopwatchTime.Text = submission.StopwatchTime
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


    Private Async Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        Using client As New HttpClient()
            Dim response As HttpResponseMessage = Await client.DeleteAsync($"http://localhost:3000/delete?index={currentIndex}")
            If response.IsSuccessStatusCode Then
                MessageBox.Show("Submission deleted successfully!")
                If submissions.Count > 0 Then
                    submissions.RemoveAt(currentIndex)
                    If submissions.Count = 0 Then
                        ClearSubmissionFields()
                    Else
                        If currentIndex >= submissions.Count Then
                            currentIndex = submissions.Count - 1
                        End If
                        DisplaySubmission(currentIndex)
                    End If
                Else
                    txtName.Clear()
                    txtEmail.Clear()
                    txtPhone.Clear()
                    txtGithub.Clear()
                    lblStopwatchTime.Text = "00:00:00"
                End If
            Else
                MessageBox.Show("Failed to delete submission.")
            End If
        End Using
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click
        ' Enable editing controls or switch to editable text boxes
        txtName.ReadOnly = False
        txtEmail.ReadOnly = False
        txtPhone.ReadOnly = False
        txtGithub.ReadOnly = False

        ' Show save button or update button to save changes
        btnSaveChanges.Visible = True
        btnEdit.Visible = False
    End Sub

    Private Async Sub btnSaveChanges_Click(sender As Object, e As EventArgs) Handles btnSaveChanges.Click
        Dim updatedSubmission As New Submission2() With {
            .Name = txtName.Text,
            .Email = txtEmail.Text,
            .Phone = txtPhone.Text,
            .GitHubLink = txtGithub.Text,
            .StopwatchTime = lblStopwatchTime.Text
        }

        Using client As New HttpClient()
            Dim json As String = JsonConvert.SerializeObject(updatedSubmission)
            Dim content As New StringContent(json, Encoding.UTF8, "application/json")
            Dim response As HttpResponseMessage = Await client.PutAsync($"http://localhost:3000/edit?index={currentIndex}", content)
            If response.IsSuccessStatusCode Then
                MessageBox.Show("Submission updated successfully!")
                submissions(currentIndex) = updatedSubmission
            Else
                MessageBox.Show("Failed to update submission.")
            End If
        End Using

        ' Disable editing controls or switch back to read-only state
        txtName.ReadOnly = True
        txtEmail.ReadOnly = True
        txtPhone.ReadOnly = True
        txtGithub.ReadOnly = True

        ' Hide save button or update button
        btnSaveChanges.Visible = False
        btnEdit.Visible = True
    End Sub
    Private Sub ClearSubmissionFields()
        txtName.Clear()
        txtEmail.Clear()
        txtPhone.Clear()
        txtGithub.Clear()
        lblStopwatchTime.Clear()
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Dim searchText As String = txtSearchEmail.Text.Trim()

        If String.IsNullOrEmpty(searchText) Then
            MessageBox.Show("Please enter an email ID to search.")
            Return
        End If

        Dim foundIndex As Integer = -1

        For i As Integer = 0 To submissions.Count - 1
            If submissions(i).Email.Equals(searchText, StringComparison.OrdinalIgnoreCase) Then
                foundIndex = i
                Exit For
            End If
        Next

        If foundIndex <> -1 Then
            currentIndex = foundIndex
            DisplaySubmission(currentIndex)
        Else
            MessageBox.Show($"Submission with email ID '{searchText}' not found.")
        End If
    End Sub

End Class

' Define the Submission class (same as before)
Public Class Submission2
    Public Property Name As String
    Public Property Email As String
    Public Property Phone As String
    Public Property GitHubLink As String
    Public Property StopwatchTime As String
End Class
