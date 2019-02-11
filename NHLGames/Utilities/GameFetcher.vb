Imports NHLGames.Objects

Namespace Utilities
    Public Class GameFetcher
        Public Shared Sub StreamingProgress()
            ResetLoadingProgress()
            NHLGamesMetro.FormInstance.spnStreaming.Visible = NHLGamesMetro.SpnStreamingVisible
            If NHLGamesMetro.SpnStreamingValue < NHLGamesMetro.FormInstance.spnStreaming.Maximum And
               NHLGamesMetro.SpnStreamingValue >= 0 Then
                NHLGamesMetro.FormInstance.spnStreaming.Value = NHLGamesMetro.SpnStreamingValue
            End If

            If NHLGamesMetro.FormInstance.spnStreaming.Value > 0 Then
                NHLGamesMetro.FormInstance.spnStreaming.Visible = True
            Else
                NHLGamesMetro.SpnStreamingVisible = False
                NHLGamesMetro.FormInstance.spnStreaming.Visible = NHLGamesMetro.SpnStreamingVisible
            End If
        End Sub

        Private Shared Sub ResetStreaminProgress()
            NHLGamesMetro.SpnStreamingVisible = False
            NHLGamesMetro.SpnStreamingValue = 0
            NHLGamesMetro.FormInstance.spnStreaming.Value = 0
            NHLGamesMetro.FormInstance.spnStreaming.Visible = False
        End Sub

        Private Shared Sub ResetLoadingProgress()
            NHLGamesMetro.SpnLoadingVisible = False
            NHLGamesMetro.SpnLoadingValue = 0
            NHLGamesMetro.FormInstance.spnLoading.Value = 0
            NHLGamesMetro.FormInstance.spnLoading.Visible = False
        End Sub

        Public Shared Sub LoadingProgress()
            ResetStreaminProgress()
            NHLGamesMetro.FormInstance.spnLoading.Visible = NHLGamesMetro.SpnLoadingVisible
            If NHLGamesMetro.SpnLoadingValue < NHLGamesMetro.FormInstance.spnLoading.Maximum And
               NHLGamesMetro.SpnLoadingValue >= 0 Then
                NHLGamesMetro.FormInstance.spnLoading.Value = NHLGamesMetro.SpnLoadingValue
            End If

            If NHLGamesMetro.FormInstance.spnLoading.Value > 0 Then
                InvokeElement.SetGameTabControls(False)
                NHLGamesMetro.FormInstance.lblNoGames.Visible = False
            Else
                NHLGamesMetro.SpnLoadingVisible = False
                NHLGamesMetro.FormInstance.spnLoading.Visible = NHLGamesMetro.SpnLoadingVisible
                InvokeElement.SetGameTabControls(True)
                If (NHLGamesMetro.FormInstance.flpGames.Controls.Count = 0) Then
                    NHLGamesMetro.FormInstance.lblNoGames.Visible = True
                Else
                    NHLGamesMetro.FormInstance.lblNoGames.Visible = False
                End If
            End If
        End Sub

        Public Shared Async Sub LoadGames()

            NHLGamesMetro.SpnLoadingValue = 1
            NHLGamesMetro.SpnLoadingVisible = True

            If Not NHLGamesMetro.FormLoaded Then
                NHLGamesMetro.SpnLoadingVisible = False
                NHLGamesMetro.SpnLoadingValue = 0
                Return
            End If

            NHLGamesMetro.GamesDict.Clear()

            Dim games As Game()

            InvokeElement.SetFormStatusLabel(NHLGamesMetro.RmText.GetString("msgLoadingGames"))

            Dim gm = New GameManager()
            Try
                games = Await gm.GetGamesAsync()

                NHLGamesMetro.SpnLoadingValue = NHLGamesMetro.SpnLoadingMaxValue - 1
                Await Task.Delay(100)

                If games IsNot Nothing Then
                    AddGamesToDict(SortGames(games))
                End If

                InvokeElement.NewGamesFound(NHLGamesMetro.GamesDict.Values.ToList())
                InvokeElement.SetFormStatusLabel(String.Format(NHLGamesMetro.RmText.GetString("msgGamesFound"),
                                                               NHLGamesMetro.GamesDict.Values.Count.ToString()))

            Catch ex As Exception
                Console.WriteLine(ex.ToString())
                Return
            Finally
                gm.Dispose()
            End Try

            NHLGamesMetro.SpnLoadingVisible = False
            NHLGamesMetro.SpnLoadingValue = 0
        End Sub

        Private Shared Function SortGames(games As Game()) As List(Of Game)
            If NHLGamesMetro.TodayLiveGamesFirst Then
                Return games.OrderBy(Of Boolean)(Function(val) val.IsUnplayable).
                    ThenBy(Of Boolean)(Function(val) val.IsEnded).
                    ThenBy(Of Long)(Function(val) val.GameDate.Ticks).ToList()
            Else
                Return games.OrderBy(Of Boolean)(Function(val) val.IsUnplayable).
                    ThenBy(Of Long)(Function(val) val.GameDate.Ticks).ToList()
            End If
        End Function

        Private Shared Sub AddGamesToDict(games As List(Of Game))
            For Each game As Game In games
                If NHLGamesMetro.GamesDict.Keys.Contains(game.GameId) Then Continue For
                NHLGamesMetro.GamesDict.Add(game.GameId, game)
            Next
        End Sub
    End Class
End Namespace
