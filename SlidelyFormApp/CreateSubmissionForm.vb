Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json

Public Class CreateSubmissionForm
    Inherits Form

    Private stopwatchRunning As Boolean = False
    Private stopwatchTime As TimeSpan = TimeSpan.Zero
    Private stopwatchTimer As New Timer()

    Public Sub New()
        InitializeComponent()
        AddHandler stopwatchTimer.Tick, AddressOf UpdateStopwatch
        stopwatchTimer.Interval = 1000 ' 1 second
    End Sub

    Private Sub CreateSubmissionForm_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.Control AndAlso e.KeyCode = Keys.T Then
            btnToggleStopwatch.PerformClick()
        ElseIf e.Control AndAlso e.KeyCode = Keys.S Then
            btnSubmit.PerformClick()
        End If
    End Sub

    Private Sub btnToggleStopwatch_Click(sender As Object, e As EventArgs) Handles btnToggleStopwatch.Click
        If stopwatchRunning Then
            stopwatchTimer.Stop()
        Else
            stopwatchTimer.Start()
        End If
        stopwatchRunning = Not stopwatchRunning
    End Sub

    Private Sub UpdateStopwatch(sender As Object, e As EventArgs)
        stopwatchTime = stopwatchTime.Add(TimeSpan.FromSeconds(1))
        lblStopwatchTime.Text = stopwatchTime.ToString("hh\:mm\:ss")
    End Sub

    Private Async Sub btnSubmit_Click(sender As Object, e As EventArgs) Handles btnSubmit.Click
        Dim submission As New Submission() With {
            .Name = txtName.Text,
            .Email = txtEmail.Text,
            .Phone = txtPhone.Text,
            .GitHubLink = txtGithub.Text,
            .StopwatchTime = lblStopwatchTime.Text
        }
        ' Save submission to backend
        Await SaveSubmission(submission)
    End Sub

    Private Async Function SaveSubmission(submission As Submission) As Task
        ' Log the submission data to the console
        Console.WriteLine("Submitting data:")
        Console.WriteLine($"Name: {submission.Name}")
        Console.WriteLine($"Email: {submission.Email}")
        Console.WriteLine($"Phone: {submission.Phone}")
        Console.WriteLine($"GitHub Link: {submission.GitHubLink}")
        Console.WriteLine($"Stopwatch Time: {submission.StopwatchTime}")

        ' Now proceed to save submission to backend
        Using client As New HttpClient()
            Dim json As String = JsonConvert.SerializeObject(submission)
            Dim content As New StringContent(json)
            Dim contentString As String = Await content.ReadAsStringAsync()
            Console.WriteLine("StringContent: " & contentString)
            Dim response As HttpResponseMessage = Await client.PostAsync("http://localhost:3000/submit", content)
            If response.IsSuccessStatusCode Then
                ClearSubmissionFields()
                MessageBox.Show("Submission successful!")
            Else
                MessageBox.Show("Failed to submit data.")
            End If
        End Using
    End Function
    Private Sub ClearSubmissionFields()
        txtName.Clear()
        txtEmail.Clear()
        txtPhone.Clear()
        txtGithub.Clear()
        lblStopwatchTime.Clear()
    End Sub
End Class

' Define the Submission class
Public Class Submission
    Public Property Name As String
    Public Property Email As String
    Public Property Phone As String
    Public Property GitHubLink As String
    Public Property StopwatchTime As String
End Class
