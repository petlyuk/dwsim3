'    Copyright 2008 Daniel Wagner O. de Medeiros
'
'    This file is part of DWSIM.
'
'    DWSIM is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    DWSIM is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with DWSIM.  If not, see <http://www.gnu.org/licenses/>.

Imports DWSIM.DWSIM.SimulationObjects.UnitOps.Auxiliary.SepOps
Imports DWSIM.DWSIM.SimulationObjects.UnitOps
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.IO


Public Class UIInitialEstimatesEditorForm

    Dim dc As Column
    Dim form As FormFlowsheet

    Dim cb1, cb2, cb3 As Object
    Public _ie As InitialEstimates
    Public pathsep As Char

    Dim _ies As InitialEstimates

    Dim loaded As Boolean = False
    Dim cvt As DWSIM.SistemasDeUnidades.Conversor
    Dim su As DWSIM.SistemasDeUnidades.Unidades
    Dim nf As String

    Private Sub UIInitialEstimatesEditorForm_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed

    End Sub

    Private Sub UIInitialEstimatesEditorForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        pathsep = Path.DirectorySeparatorChar

        cvt = New DWSIM.SistemasDeUnidades.Conversor()
        form = My.Application.ActiveSimulation
        dc = form.Collections.ObjectCollection(form.FormSurface.FlowsheetDesignSurface.SelectedObject.Name)
        nf = form.Options.NumberFormat
        su = form.Options.SelectedUnitSystem

        dgvv.Columns(1).HeaderText += " (" & form.Options.SelectedUnitSystem.spmp_temperature & ")"
        dgvv.Columns(2).HeaderText += " (" & form.Options.SelectedUnitSystem.spmp_molarflow & ")"
        dgvv.Columns(3).HeaderText += " (" & form.Options.SelectedUnitSystem.spmp_molarflow & ")"

        dgvv.Rows.Clear()
        Dim i As Integer = 0
        Dim count As Integer = dc.Stages.Count
        For Each st As Stage In dc.Stages
            dgvv.Rows.Add(New Object() {dc.Stages(i).Name, Format(cvt.ConverterDoSI(su.spmp_temperature, dc.InitialEstimates.StageTemps(i).Value), nf), Format(cvt.ConverterDoSI(su.spmp_molarflow, dc.InitialEstimates.VapMolarFlows(i).Value), nf), Format(cvt.ConverterDoSI(su.spmp_molarflow, dc.InitialEstimates.LiqMolarFlows(i).Value), nf)})
            dgvv.Rows(dgvv.Rows.Count - 1).HeaderCell.Value = i
            i += 1
        Next

        Dim j As Integer = 0
        For Each cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties In form.Options.SelectedComponents.Values
            dgvcl.Columns.Add(cp.Name, DWSIM.App.GetComponentName(cp.Name))
            dgvcv.Columns.Add(cp.Name, DWSIM.App.GetComponentName(cp.Name))
            j = j + 1
        Next

        If dc.InitialEstimates.LiqCompositions.Count = 0 Then
            dc.RebuildEstimates()
        Else
            If form.Options.SelectedComponents.Count <> dc.InitialEstimates.LiqCompositions(0).Count Then
                dc.RebuildEstimates()
            End If
        End If


        i = 0
        Dim ob(CInt(j.ToString)) As Object
        For Each st As Stage In dc.Stages
            j = 1
            ob(0) = dc.Stages(i).Name
            For Each cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties In form.Options.SelectedComponents.Values
                ob(j) = Format(dc.InitialEstimates.LiqCompositions(i)(cp.Name).Value, nf)
                j = j + 1
            Next
            dgvcl.Rows.Add(ob)
            dgvcl.Rows(dgvcl.Rows.Count - 1).HeaderCell.Value = i
            i += 1
        Next

        i = 0
        For Each st As Stage In dc.Stages
            j = 1
            ob(0) = dc.Stages(i).Name
            For Each cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties In form.Options.SelectedComponents.Values
                ob(j) = Format(dc.InitialEstimates.VapCompositions(i)(cp.Name).Value, nf)
                j = j + 1
            Next
            dgvcv.Rows.Add(ob)
            dgvcv.Rows(dgvcv.Rows.Count - 1).HeaderCell.Value = i
            i += 1
        Next

        loaded = True

    End Sub

    Private Sub ToolStripButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click
        If dgvv.SelectedCells.Count > 0 Then
            cb1 = dgvv.SelectedCells(0).Value
        End If
    End Sub

    Private Sub ToolStripButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton2.Click
        If dgvv.SelectedCells.Count > 0 Then
            If Not dgvv.SelectedCells(0).ReadOnly Then
                dgvv.SelectedCells(0).Value = cb1
            End If
        End If
    End Sub

    Private Sub ToolStripButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton5.Click
        If dgvv.SelectedCells.Count > 0 Then
            If Not dgvv.SelectedCells(0).ReadOnly Then
                dgvv.SelectedCells(0).Value = Nothing
            End If
        End If
    End Sub

    Private Sub ToolStripButton6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton6.Click
        If dgvv.SelectedCells.Count > 0 Then
            dgvv.SelectedCells(0).Tag = "LOCKED"
            dgvv.SelectedCells(0).Style.BackColor = Color.WhiteSmoke
        End If
    End Sub

    Private Sub ToolStripButton7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton7.Click
        If dgvv.SelectedCells.Count > 0 Then
            dgvv.SelectedCells(0).Tag = ""
            dgvv.SelectedCells(0).Style.BackColor = Color.White
        End If
    End Sub

    Private Sub ToolStripButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton3.Click
        If dgvcl.SelectedCells.Count > 0 Then
            cb2 = dgvcl.SelectedCells(0).Value
        End If
    End Sub

    Private Sub ToolStripButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton4.Click
        If dgvcl.SelectedCells.Count > 0 Then
            If Not dgvcl.SelectedCells(0).ReadOnly Then
                dgvcl.SelectedCells(0).Value = cb2
            End If
        End If
    End Sub

    Private Sub ToolStripButton8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton8.Click
        If dgvcl.SelectedCells.Count > 0 Then
            If Not dgvcl.SelectedCells(0).ReadOnly Then
                dgvcl.SelectedCells(0).Value = Nothing
            End If
        End If
    End Sub

    Private Sub ToolStripButton9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton9.Click
        If dgvcl.SelectedCells.Count > 0 Then
            dgvcl.SelectedCells(0).Tag = "LOCKED"
            dgvcl.SelectedCells(0).Style.BackColor = Color.WhiteSmoke
        End If
    End Sub

    Private Sub ToolStripButton10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton10.Click
        If dgvcl.SelectedCells.Count > 0 Then
            dgvcl.SelectedCells(0).Tag = ""
            dgvcl.SelectedCells(0).Style.BackColor = Color.White
        End If
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim myStream As System.IO.FileStream
        If Me.ofd1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            myStream = Me.ofd1.OpenFile()
            If Not (myStream Is Nothing) Then
                Dim mySerializer As BinaryFormatter = New BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())
                Try
                    _ies = DirectCast(mySerializer.Deserialize(myStream), InitialEstimates)
                    Me.TextBox1.Text = Me.ofd1.FileName.ToString
                Catch ex As System.Runtime.Serialization.SerializationException
                    MessageBox.Show(ex.Message)
                Finally
                    myStream.Close()
                End Try
            End If
        End If
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click

        Dim myStream As System.IO.FileStream
        If Me.sfd1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            myStream = Me.sfd1.OpenFile()
            If Not (myStream Is Nothing) Then
                Dim mySerializer As BinaryFormatter = New BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())
                Try
                    mySerializer.Serialize(myStream, dc.InitialEstimates)
                    Me.TextBox1.Text = Me.sfd1.FileName.ToString
                Catch ex As System.Runtime.Serialization.SerializationException
                    MessageBox.Show(ex.Message)
                Finally
                    myStream.Close()
                End Try
            End If
        End If

    End Sub

    Private Sub dgvv_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvv.CellValueChanged
        If loaded Then
            Dim value As Object = dgvv.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
            Dim count As Integer = dc.Stages.Count
            Select Case e.ColumnIndex
                Case 1
                    dc.InitialEstimates.StageTemps(e.RowIndex).Value = cvt.ConverterParaSI(su.spmp_temperature, value)
                Case 2
                    dc.InitialEstimates.VapMolarFlows(e.RowIndex).Value = cvt.ConverterParaSI(su.spmp_molarflow, value)
                Case 3
                    dc.InitialEstimates.LiqMolarFlows(e.RowIndex).Value = cvt.ConverterParaSI(su.spmp_molarflow, value)
            End Select
        End If
    End Sub

    Private Sub dgvc_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvcl.CellValueChanged
        If loaded Then
            Dim value As Object = dgvcl.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
            Dim count As Integer = dc.Stages.Count
            Dim colname As String = dgvcl.Columns(e.ColumnIndex).Name
            dc.InitialEstimates.LiqCompositions(e.RowIndex)(colname).Value = value
        End If
    End Sub

    Private Sub dgvc_DataError(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewDataErrorEventArgs) Handles dgvcl.DataError
        e.Cancel = True
    End Sub

    Private Sub dgvv_DataError(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewDataErrorEventArgs) Handles dgvv.DataError
        e.Cancel = True
    End Sub

    Private Sub ToolStripButton21_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton21.Click
        If dgvcv.SelectedCells.Count > 0 Then
            cb3 = dgvcv.SelectedCells(0).Value
        End If
    End Sub

    Private Sub ToolStripButton22_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton22.Click
        If dgvcv.SelectedCells.Count > 0 Then
            If Not dgvcv.SelectedCells(0).ReadOnly Then
                dgvcv.SelectedCells(0).Value = cb3
            End If
        End If
    End Sub

    Private Sub ToolStripButton23_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton23.Click
        If dgvcv.SelectedCells.Count > 0 Then
            If Not dgvcv.SelectedCells(0).ReadOnly Then
                dgvcv.SelectedCells(0).Value = Nothing
            End If
        End If
    End Sub

    Private Sub ToolStripButton24_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton24.Click
        If dgvcv.SelectedCells.Count > 0 Then
            dgvcv.SelectedCells(0).Tag = "LOCKED"
            dgvcv.SelectedCells(0).Style.BackColor = Color.WhiteSmoke
        End If
    End Sub

    Private Sub ToolStripButton25_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton25.Click
        If dgvcv.SelectedCells.Count > 0 Then
            dgvcv.SelectedCells(0).Tag = ""
            dgvcv.SelectedCells(0).Style.BackColor = Color.White
        End If
    End Sub

    Private Sub dgvcv_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvcv.CellValueChanged
        If loaded Then
            Dim value As Object = dgvcv.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
            Dim count As Integer = dc.Stages.Count
            Dim colname As String = dgvcv.Columns(e.ColumnIndex).Name
            dc.InitialEstimates.VapCompositions(e.RowIndex)(colname).Value = value
        End If
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Dim i, j As Integer
        For i = 0 To dc.NumberOfStages - 1
            dgvv.Rows(i).Cells(1).Value = Format(cvt.ConverterDoSI(su.spmp_temperature, dc.Tf(i)), nf)
            dgvv.Rows(i).Cells(2).Value = Format(cvt.ConverterDoSI(su.spmp_molarflow, dc.Vf(i)), nf)
            dgvv.Rows(i).Cells(3).Value = Format(cvt.ConverterDoSI(su.spmp_molarflow, dc.Lf(i)), nf)
            For j = 0 To dc.compids.Count - 1
                dgvcl.Rows(i).Cells(j + 1).Value = Format(dc.xf(i)(j), nf)
                dgvcv.Rows(i).Cells(j + 1).Value = Format(dc.yf(i)(j), nf)
            Next
        Next
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        loaded = False

        cvt = New DWSIM.SistemasDeUnidades.Conversor()
        form = My.Application.ActiveSimulation
        dc = form.Collections.ObjectCollection(form.FormSurface.FlowsheetDesignSurface.SelectedObject.Name)
        nf = form.Options.NumberFormat
        su = form.Options.SelectedUnitSystem

        'dgvv.Columns(1).HeaderText += " (" & form.Options.SelectedUnitSystem.spmp_temperature & ")"
        'dgvv.Columns(2).HeaderText += " (" & form.Options.SelectedUnitSystem.spmp_molarflow & ")"

        dgvv.Rows.Clear()
        Dim i As Integer = 0
        Dim count As Integer = dc.Stages.Count
        For Each st As Stage In dc.Stages
            dgvv.Rows.Add(New Object() {dc.Stages(i).Name, Format(cvt.ConverterDoSI(su.spmp_temperature, _ies.StageTemps(i).Value), nf), Format(cvt.ConverterDoSI(su.spmp_molarflow, _ies.VapMolarFlows(i).Value), nf), Format(cvt.ConverterDoSI(su.spmp_molarflow, _ies.LiqMolarFlows(i).Value), nf)})
            dgvv.Rows(dgvv.Rows.Count - 1).HeaderCell.Value = i
            i += 1
        Next

        Dim j As Integer = 0
        For Each cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties In form.Options.SelectedComponents.Values
            dgvcl.Columns.Add(cp.Name, DWSIM.App.GetComponentName(cp.Name))
            dgvcv.Columns.Add(cp.Name, DWSIM.App.GetComponentName(cp.Name))
            j = j + 1
        Next

        i = 0
        Dim ob(CInt(j.ToString)) As Object
        For Each st As Stage In dc.Stages
            j = 1
            ob(0) = dc.Stages(i).Name
            For Each cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties In form.Options.SelectedComponents.Values
                ob(j) = Format(_ies.LiqCompositions(i)(cp.Name).Value, nf)
                j = j + 1
            Next
            dgvcl.Rows.Add(ob)
            dgvcl.Rows(dgvcl.Rows.Count - 1).HeaderCell.Value = i
            i += 1
        Next

        i = 0
        For Each st As Stage In dc.Stages
            j = 1
            ob(0) = dc.Stages(i).Name
            For Each cp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties In form.Options.SelectedComponents.Values
                ob(j) = Format(_ies.VapCompositions(i)(cp.Name).Value, nf)
                j = j + 1
            Next
            dgvcv.Rows.Add(ob)
            dgvcv.Rows(dgvcv.Rows.Count - 1).HeaderCell.Value = i
            i += 1
        Next

        loaded = True

    End Sub
End Class