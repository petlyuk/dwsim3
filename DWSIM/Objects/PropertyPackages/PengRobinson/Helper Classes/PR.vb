'    Peng-Robinson Property Package 
'    Copyright 2008-2014 Daniel Wagner O. de Medeiros
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

Imports DWSIM.DWSIM.MathEx
Imports Cudafy
Imports Cudafy.Translator
Imports Cudafy.Host
Imports System.Math

Namespace DWSIM.SimulationObjects.PropertyPackages.ThermoPlugs

    <System.Serializable()> Public Class PR

        Inherits DWSIM.SimulationObjects.PropertyPackages.ThermoPlug

        Shared Function ReturnParameters(ByVal T As Double, ByVal P As Double, ByVal Vx As Array, ByVal VKij As Object, ByVal VTc As Array, ByVal VPc As Array, ByVal Vw As Array)

            Dim n, R, coeff(3) As Double
            Dim Vant(0, 4) As Double
            Dim criterioOK As Boolean = False
            Dim AG, BG, aml, bml As Double

            n = UBound(Vx)

            Dim ai(n), bi(n), ci(n), tmp(n + 1), a(n, n), b(n, n)
            Dim aml2(n), amv2(n), LN_CF(n), PHI(n) As Double
            Dim Tc(n), Pc(n), W(n), alpha(n), m(n), Tr(n)

            R = 8.314

            Dim i, j As Integer
            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                ci(i) = 0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                aml2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            i = 0
            aml = 0
            Do
                j = 0
                Do
                    aml = aml + Vx(i) * Vx(j) * a(i, j)
                    aml2(i) = aml2(i) + Vx(j) * a(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            bml = 0
            Do
                bml = bml + Vx(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            AG = aml * P / (R * T) ^ 2
            BG = bml * P / (R * T)

            Dim _zarray As ArrayList, _mingz As Object, Z As Double
            _zarray = CalcZ(T, P, Vx, VKij, VTc, VPc, Vw)
            _mingz = ZtoMinG(_zarray.ToArray, T, P, Vx, VKij, VTc, VPc, Vw)
            Z = _zarray(_mingz(0))

            Dim aux1 = -R / 2 * (0.45724 / T) ^ 0.5
            i = 0
            Dim aux2 = 0.0#
            Do
                j = 0
                Do
                    aux2 += Vx(i) * Vx(j) * (1 - VKij(i, j)) * (ci(j) * (ai(i) * Tc(j) / Pc(j)) ^ 0.5 + ci(i) * (ai(j) * Tc(i) / Pc(i)) ^ 0.5)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Dim dadT = aux1 * aux2

            Return New Double() {aml, bml, Z * R * T / P, dadT}

        End Function

        Shared Function ZtoMinG(ByVal Z_ As Array, ByVal T As Double, ByVal P As Double, ByVal Vz As Array, ByVal VKij As Object, ByVal VTc As Array, ByVal VPc As Array, ByVal Vw As Array) As Object

            DWSIM.App.WriteToConsole("PR min-G root finder (Z) for T = " & T & " K, P = " & P & " Pa and Z = " & DirectCast(Z_, Object()).ToArrayString, 3)

            Dim S, H, Z As Double

            Dim ai(), bi(), ci() As Double
            Dim n, R, dadT As Double
            Dim Tc(), Pc(), Vc(), w(), Zc(), alpha(), m(), a(,), b(,), Tr() As Double
            Dim i, j, k, l As Integer

            n = UBound(Vz)

            Dim G(UBound(Z_)) As Double

            ReDim ai(n), bi(n), ci(n), a(n, n), b(n, n)
            ReDim Tc(n), Pc(n), Vc(n), Zc(n), w(n), alpha(n), m(n), Tr(n)

            R = 8.314

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                w(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                alpha(i) = (1 + (0.37464 + 1.54226 * w(i) - 0.26992 * w(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                ci(i) = 0.37464 + 1.54226 * w(i) - 0.26992 * w(i) ^ 2
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim am = 0.0#
            Do
                j = 0
                Do
                    am = am + Vz(i) * Vz(j) * a(i, j)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bm = 0.0#
            Do
                bm = bm + Vz(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            Dim AG1 = am * P / (R * T) ^ 2
            Dim BG1 = bm * P / (R * T)

            l = 0
            For Each Z In Z_

                Dim V = (Z * R * T / P) ' m3/mol

                Dim tmp1 = 1 / V / 1000

                Dim aux1 = -R / 2 * (0.45724 / T) ^ 0.5
                i = 0
                Dim aux2 = 0.0#
                Do
                    j = 0
                    Do
                        aux2 += Vz(i) * Vz(j) * (1 - VKij(i, j)) * (ci(j) * (ai(i) * Tc(j) / Pc(j)) ^ 0.5 + ci(i) * (ai(j) * Tc(i) / Pc(i)) ^ 0.5)
                        j = j + 1
                    Loop Until j = n + 1
                    i = i + 1
                Loop Until i = n + 1

                dadT = aux1 * aux2

                Dim uu, ww As Double
                uu = 2
                ww = -1

                Dim DAres = am / (bm * (uu ^ 2 - 4 * ww) ^ 0.5) * Math.Log((2 * Z + BG1 * (uu - (uu ^ 2 - 4 * ww) ^ 0.5)) / (2 * Z + BG1 * (uu + (uu ^ 2 - 4 * ww) ^ 0.5))) - R * T * Math.Log((Z - BG1) / Z) - R * T * Math.Log(V * 101325 / (R * 298.15))
                Dim V0 As Double = R * 298.15 / 101325
                Dim DSres = R * Math.Log((Z - BG1) / Z) + R * Math.Log(Z) - 1 / (8 ^ 0.5 * bm) * dadT * Math.Log((2 * Z + BG1 * (2 - 8 ^ 0.5)) / (2 * Z + BG1 * (2 + 8 ^ 0.5)))
                Dim DHres = DAres + T * (DSres) + R * T * (Z - 1)

                H = DHres

                S = DSres

                G(l) = H - T * S

                If j = 0 Then
                    k = 0
                Else
                    i = 0
                    Do
                        If G(l) <= G(k) Then k = l
                        i = i + 1
                    Loop Until i = UBound(G) + 1
                End If

                l = l + 1

            Next

            DWSIM.App.WriteToConsole("Result: Min-G Z Index = " & k, 3)

            Return New Object() {k, G(k)}

        End Function

        Public Overrides Function CalcLnFug(ByVal T As Double, ByVal P As Double, ByVal Vx As Array, ByVal VKij As Object, ByVal VTc As Array, ByVal VPc As Array, ByVal Vw As Array, Optional ByVal otherargs As Object = Nothing, Optional ByVal forcephase As String = "") As Double()

            If My.Settings.EnableGPUProcessing Then
                Return CalcLnFugGPU(T, P, Vx, VKij, VTc, VPc, Vw, otherargs, forcephase)
            Else
                Return CalcLnFugCPU(T, P, Vx, VKij, VTc, VPc, Vw, otherargs, forcephase)
            End If

        End Function

        Private Function CalcLnFugCPU(ByVal T As Double, ByVal P As Double, ByVal Vx As Array, ByVal VKij As Object, ByVal VTc As Array, ByVal VPc As Array, ByVal Vw As Array, Optional ByVal otherargs As Object = Nothing, Optional ByVal forcephase As String = "")

            Dim n, R, coeff(3) As Double
            Dim Vant(0, 4) As Double
            Dim criterioOK As Boolean = False
            Dim AG, BG, aml, bml As Double
            Dim t1, t2, t3, t4, t5 As Double

            n = UBound(Vx)

            Dim ai(n), bi(n), tmp(n + 1), a(n, n), b(n, n)
            Dim aml2(n), amv2(n), LN_CF(n), PHI(n) As Double
            Dim Tc(n), Pc(n), W(n), alpha(n), m(n), Tr(n)

            R = 8.314

            Dim i, j As Integer
            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                aml2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            i = 0
            aml = 0
            Do
                j = 0
                Do
                    aml = aml + Vx(i) * Vx(j) * a(i, j)
                    aml2(i) = aml2(i) + Vx(j) * a(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            bml = 0
            Do
                bml = bml + Vx(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            AG = aml * P / (R * T) ^ 2
            BG = bml * P / (R * T)

            Dim _zarray As ArrayList, _mingz As Object, Z As Double

            _zarray = CalcZ(T, P, Vx, VKij, VTc, VPc, Vw)
            If forcephase <> "" Then
                If forcephase = "L" Then
                    Z = Common.Min(_zarray.ToArray())
                ElseIf forcephase = "V" Then
                    Z = Common.Max(_zarray.ToArray())
                End If
            Else
                _mingz = ZtoMinG(_zarray.ToArray, T, P, Vx, VKij, VTc, VPc, Vw)
                Z = _zarray(_mingz(0))
            End If

            Dim Pcorr As Double = P
            Dim ZP As Double() = CheckRoot(Z, aml, bml, P, T, forcephase)
            Z = ZP(0)
            Pcorr = ZP(1)

            i = 0
            Do
                t1 = bi(i) * (Z - 1) / bml
                t2 = -Math.Log(Z - BG)
                t3 = AG * (2 * aml2(i) / aml - bi(i) / bml)
                t4 = Math.Log((Z + (1 + 2 ^ 0.5) * BG) / (Z + (1 - 2 ^ 0.5) * BG))
                t5 = 2 * 2 ^ 0.5 * BG
                LN_CF(i) = t1 + t2 - (t3 * t4 / t5)
                LN_CF(i) = LN_CF(i) + Math.Log(Pcorr / P)
                i = i + 1
            Loop Until i = n + 1

            Return LN_CF

        End Function

        Private Function CalcLnFugGPU(ByVal T As Double, ByVal P As Double, ByVal Vx As Array, ByVal VKij As Double(,), ByVal VTc As Array, ByVal VPc As Array, ByVal Vw As Array, Optional ByVal otherargs As Object = Nothing, Optional ByVal forcephase As String = "")

            Dim n, R, coeff(3) As Double
            Dim Vant(0, 4) As Double
            Dim criterioOK As Boolean = False
            Dim AG, BG, aml, bml As Double
            Dim t1, t2, t3, t4, t5 As Double

            n = UBound(Vx)

            Dim ai(n), bi(n), tmp(n + 1), a(n, n), b(n, n) As Double
            Dim aml2(n), amv2(n), LN_CF(n), PHI(n) As Double
            Dim Tc(n), Pc(n), W(n), alpha(n), m(n), Tr(n) As Double

            R = 8.314

            Dim i As Integer
            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            Dim aml_temp(n), aml2_temp(n), bml_temp(n) As Double

            pr_gpu_func(n, Vx, VKij, Tc, Pc, W, T, alpha, ai, bi, a, aml_temp, bml_temp, aml2_temp)

            aml2 = aml2_temp
            aml = MathEx.Common.Sum(aml_temp)
            bml = MathEx.Common.Sum(bml_temp)

            AG = aml * P / (R * T) ^ 2
            BG = bml * P / (R * T)

            Dim _zarray As ArrayList, _mingz As Object, Z As Double

            _zarray = CalcZ(T, P, Vx, VKij, VTc, VPc, Vw)
            If forcephase <> "" Then
                If forcephase = "L" Then
                    Z = Common.Min(_zarray.ToArray())
                ElseIf forcephase = "V" Then
                    Z = Common.Max(_zarray.ToArray())
                End If
            Else
                _mingz = ZtoMinG(_zarray.ToArray, T, P, Vx, VKij, VTc, VPc, Vw)
                Z = _zarray(_mingz(0))
            End If

            Dim Pcorr As Double = P
            Dim ZP As Double() = CheckRoot(Z, aml, bml, P, T, forcephase)
            Z = ZP(0)
            Pcorr = ZP(1)

            i = 0
            Do
                t1 = bi(i) * (Z - 1) / bml
                t2 = -Math.Log(Z - BG)
                t3 = AG * (2 * aml2(i) / aml - bi(i) / bml)
                t4 = Math.Log((Z + (1 + 2 ^ 0.5) * BG) / (Z + (1 - 2 ^ 0.5) * BG))
                t5 = 2 * 2 ^ 0.5 * BG
                LN_CF(i) = t1 + t2 - (t3 * t4 / t5)
                LN_CF(i) = LN_CF(i) + Math.Log(Pcorr / P)
                i = i + 1
            Loop Until i = n + 1

            Return LN_CF

        End Function

        Public Shared Sub pr_gpu_func(n As Integer, Vx As Double(), VKij As Double(,), Tc As Double(), Pc As Double(), w As Double(), T As Double, alpha As Double(), ai As Double(), bi As Double(), a As Double(,), aml_temp As Double(), bml_temp As Double(), aml2_temp As Double())

            Dim gpu As GPGPU = My.MyApplication.gpu

            If gpu.IsMultithreadingEnabled Then gpu.Lock()

            ' allocate the memory on the GPU
            Dim dev_alpha As Double() = gpu.Allocate(Of Double)(alpha)
            Dim dev_ai As Double() = gpu.Allocate(Of Double)(ai)
            Dim dev_bi As Double() = gpu.Allocate(Of Double)(bi)
            Dim dev_Tc As Double() = gpu.Allocate(Of Double)(Tc)
            Dim dev_Pc As Double() = gpu.Allocate(Of Double)(Pc)
            Dim dev_W As Double() = gpu.Allocate(Of Double)(w)
            Dim dev_a As Double(,) = gpu.Allocate(Of Double)(a)
            Dim dev_vkij As Double(,) = gpu.Allocate(Of Double)(VKij)
            Dim dev_Vx As Double() = gpu.Allocate(Of Double)(Vx)
            Dim dev_aml2_temp As Double() = gpu.Allocate(Of Double)(aml2_temp)
            Dim dev_aml_temp As Double() = gpu.Allocate(Of Double)(aml_temp)
            Dim dev_bml_temp As Double() = gpu.Allocate(Of Double)(bml_temp)

            ' copy the arrays to the GPU
            gpu.CopyToDevice(alpha, dev_alpha)
            gpu.CopyToDevice(ai, dev_ai)
            gpu.CopyToDevice(bi, dev_bi)
            gpu.CopyToDevice(Tc, dev_Tc)
            gpu.CopyToDevice(Pc, dev_Pc)
            gpu.CopyToDevice(w, dev_W)
            gpu.CopyToDevice(a, dev_a)
            gpu.CopyToDevice(VKij, dev_vkij)
            gpu.CopyToDevice(Vx, dev_Vx)
            gpu.CopyToDevice(aml2_temp, dev_aml2_temp)
            gpu.CopyToDevice(aml_temp, dev_aml_temp)
            gpu.CopyToDevice(bml_temp, dev_bml_temp)

            ' launch subs
            gpu.Launch(n + 1, 1).pr_gpu_sum1(dev_alpha, dev_ai, dev_bi, dev_Tc, dev_Pc, dev_W, T)
            gpu.Launch(New dim3(n + 1, n + 1), 1).pr_gpu_sum2(dev_a, dev_ai, dev_vkij)
            gpu.Launch(n + 1, 1).pr_gpu_sum3(dev_Vx, dev_a, dev_aml_temp, dev_aml2_temp)
            gpu.Launch(n + 1, 1).pr_gpu_sum4(dev_Vx, dev_bi, dev_bml_temp)

            ' copy the arrays back from the GPU to the CPU
            gpu.CopyFromDevice(dev_alpha, alpha)
            gpu.CopyFromDevice(dev_ai, ai)
            gpu.CopyFromDevice(dev_bi, bi)
            gpu.CopyFromDevice(dev_Tc, Tc)
            gpu.CopyFromDevice(dev_Pc, Pc)
            gpu.CopyFromDevice(dev_W, w)
            gpu.CopyFromDevice(dev_a, a)
            gpu.CopyFromDevice(dev_vkij, VKij)
            gpu.CopyFromDevice(dev_Vx, Vx)
            gpu.CopyFromDevice(dev_aml2_temp, aml2_temp)
            gpu.CopyFromDevice(dev_aml_temp, aml_temp)
            gpu.CopyFromDevice(dev_bml_temp, bml_temp)

            ' free the memory allocated on the GPU
            gpu.Free(dev_alpha)
            gpu.Free(dev_ai)
            gpu.Free(dev_bi)
            gpu.Free(dev_Tc)
            gpu.Free(dev_Pc)
            gpu.Free(dev_W)
            gpu.Free(dev_a)
            gpu.Free(dev_vkij)
            gpu.Free(dev_Vx)
            gpu.Free(dev_aml2_temp)
            gpu.Free(dev_aml_temp)
            gpu.Free(dev_bml_temp)

            If gpu.IsMultithreadingEnabled Then gpu.Unlock()

        End Sub

        <Cudafy.Cudafy()> Private Shared Sub pr_gpu_sum1(thread As Cudafy.GThread, alpha As Double(), ai As Double(), bi As Double(), Tc As Double(), Pc As Double(), W As Double(), T As Double)

            Dim i As Integer = thread.blockIdx.x

            alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
            ai(i) = 0.45724 * alpha(i) * 8.314 ^ 2 * Tc(i) ^ 2 / Pc(i)
            bi(i) = 0.0778 * 8.314 * Tc(i) / Pc(i)

        End Sub

        <Cudafy.Cudafy()> Private Shared Sub pr_gpu_sum2(thread As Cudafy.GThread, a As Double(,), ai As Double(), VKij As Double(,))

            Dim i As Integer = thread.blockIdx.x
            Dim j As Integer = thread.blockIdx.y

            a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))

        End Sub

        <Cudafy.Cudafy()> Private Shared Sub pr_gpu_sum3(thread As Cudafy.GThread, Vx As Double(), a As Double(,), aml_temp As Double(), aml2_temp As Double())

            Dim i As Integer = thread.blockIdx.x

            aml_temp(i) = 0
            aml2_temp(i) = 0
            For k As Integer = 0 To Vx.Length - 1
                aml_temp(i) += Vx(i) * Vx(k) * a(i, k)
                aml2_temp(i) += Vx(k) * a(k, i)
            Next

        End Sub

        <Cudafy.Cudafy()> Private Shared Sub pr_gpu_sum4(thread As Cudafy.GThread, Vx As Double(), bi As Double(), bml_temp As Double())

            Dim i As Integer = thread.blockIdx.x

            bml_temp(i) = Vx(i) * bi(i)

        End Sub

        Shared Function CalcZ(ByVal T As Double, ByVal P As Double, ByVal Vx As Double(), ByVal VKij As Double(,), ByVal VTc As Double(), ByVal VPc As Double(), ByVal Vw As Double()) As ArrayList

            Dim n, R, coeff(3) As Double
            Dim Vant(0, 4) As Double

            n = UBound(Vx)

            Dim ai(n), bi(n), tmp(n + 1), a(n, n), b(n, n) As Double
            Dim aml2(n), amv2(n) As Double
            Dim Tc(n), Pc(n), W(n), alpha(n), m(n), Tr(n) As Double

            R = 8.314

            Dim i, j As Integer
            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                aml2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim aml = 0.0#
            Do
                j = 0
                Do
                    aml = aml + Vx(i) * Vx(j) * a(i, j)
                    aml2(i) = aml2(i) + Vx(j) * a(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bml = 0.0#
            Do
                bml = bml + Vx(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            Dim AG = aml * P / (R * T) ^ 2
            Dim BG = bml * P / (R * T)

            coeff(0) = -AG * BG + BG ^ 2 + BG ^ 3
            coeff(1) = AG - 3 * BG ^ 2 - 2 * BG
            coeff(2) = BG - 1
            coeff(3) = 1

            Dim temp1 = Poly_Roots(coeff)
            Dim tv = 0.0#
            Dim ZV, tv2 As Double

            Dim result As New ArrayList

            If temp1(0, 0) > temp1(1, 0) Then
                tv = temp1(1, 0)
                temp1(1, 0) = temp1(0, 0)
                temp1(0, 0) = tv
                tv2 = temp1(1, 1)
                temp1(1, 1) = temp1(0, 1)
                temp1(0, 1) = tv2
            End If
            If temp1(0, 0) > temp1(2, 0) Then
                tv = temp1(2, 0)
                temp1(2, 0) = temp1(0, 0)
                temp1(0, 0) = tv
                tv2 = temp1(2, 1)
                temp1(2, 1) = temp1(0, 1)
                temp1(0, 1) = tv2
            End If
            If temp1(1, 0) > temp1(2, 0) Then
                tv = temp1(2, 0)
                temp1(2, 0) = temp1(1, 0)
                temp1(1, 0) = tv
                tv2 = temp1(2, 1)
                temp1(2, 1) = temp1(1, 1)
                temp1(1, 1) = tv2
            End If

            ZV = temp1(2, 0)
            If temp1(2, 1) <> 0 Then
                ZV = temp1(1, 0)
                If temp1(1, 1) <> 0 Then
                    ZV = temp1(0, 0)
                End If
            End If

            If temp1(0, 1) = 0.0# And temp1(0, 0) > 0.0# Then result.Add(temp1(0, 0))
            If temp1(1, 1) = 0.0# And temp1(1, 0) > 0.0# Then result.Add(temp1(1, 0))
            If temp1(2, 1) = 0.0# And temp1(2, 0) > 0.0# Then result.Add(temp1(2, 0))

            Return result

        End Function

        Public Overrides Function PhaseType(ByVal T As Double, ByVal P As Double, ByVal Vx As Array, ByVal VKij As Object, ByVal VTc As Array, ByVal VPc As Array, ByVal Vw As Array, Optional ByVal otherargs As Object = Nothing)

            Dim n, R, coeff(3) As Double
            Dim Vant(0, 4) As Double
            Dim beta As Double
            Dim criterioOK As Boolean = False
            Dim AG, BG, aml, bml As Double

            n = UBound(Vx)

            Dim ai(n), bi(n), tmp(n + 1), a(n, n), b(n, n)
            Dim aml2(n), amv2(n), LN_CF(n), PHI(n) As Double
            Dim Tc(n), Pc(n), W(n), alpha(n), m(n), Tr(n)

            R = 8.314

            Dim i, j As Integer
            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                aml2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            i = 0
            aml = 0
            Do
                j = 0
                Do
                    aml = aml + Vx(i) * Vx(j) * a(i, j)
                    aml2(i) = aml2(i) + Vx(j) * a(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            bml = 0
            Do
                bml = bml + Vx(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            AG = aml * P / (R * T) ^ 2
            BG = bml * P / (R * T)

            Dim _zarray As ArrayList, _mingz As Object, Z As Double

            _zarray = CalcZ(T, P, Vx, VKij, VTc, VPc, Vw)
            _mingz = ZtoMinG(_zarray.ToArray, T, P, Vx, VKij, VTc, VPc, Vw)
            Z = _zarray(_mingz(0))

            beta = 1 / P * (1 - (BG * Z ^ 2 + AG * Z - 6 * BG ^ 2 * Z - 2 * BG * Z - 2 * AG * BG + 2 * BG ^ 2 + 2 * BG) / (Z * (3 * Z ^ 2 - 2 * Z + 2 * BG * Z + AG - 3 * BG ^ 2 - 2 * BG)))
            Dim phase As String = "Unknown"

            'If beta < 0.005 / 101322 Then phase = "L" Else phase = "V"
            'If beta > 0.9 / P And beta < 3 / P Then phase = "V" Else phase = "L"
            If Z < 0.302 Then phase = "L" Else phase = "V"

            Return New Object() {phase, beta}

        End Function

        Public Overrides Function CalcEnthalpy(ByVal phasetype As String, ByVal T As Double, ByVal P As Double, ByVal Vz As System.Array, ByVal VKij As Object, ByVal VTc As System.Array, ByVal VPc As System.Array, ByVal Vw As System.Array, ByVal Hid As Double, Optional ByVal otherargs As Object = Nothing) As Double

            Dim ai(), bi(), ci() As Double
            Dim n, R As Double
            Dim Tc(), Pc(), Vc(), w(), Zc(), alpha(), m(), a(,), b(,), Z, Tr() As Double
            Dim i, j, dadT

            n = UBound(Vz)

            ReDim ai(n), bi(n), ci(n), a(n, n), b(n, n)
            ReDim Tc(n), Pc(n), Vc(n), Zc(n), w(n), alpha(n), m(n), Tr(n)

            R = 8.314

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                w(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                alpha(i) = (1 + (0.37464 + 1.54226 * w(i) - 0.26992 * w(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                ci(i) = 0.37464 + 1.54226 * w(i) - 0.26992 * w(i) ^ 2
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim am = 0.0#
            Do
                j = 0
                Do
                    am = am + Vz(i) * Vz(j) * a(i, j)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bm = 0.0#
            Do
                bm = bm + Vz(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            Dim AG1 = am * P / (R * T) ^ 2
            Dim BG1 = bm * P / (R * T)

            Dim coeff(3) As Double

            coeff(0) = -AG1 * BG1 + BG1 ^ 2 + BG1 ^ 3
            coeff(1) = AG1 - 3 * BG1 ^ 2 - 2 * BG1
            coeff(2) = BG1 - 1
            coeff(3) = 1

            Dim temp1 = Poly_Roots(coeff)
            Dim tv
            Dim tv2
            Try

                If temp1(0, 0) > temp1(1, 0) Then
                    tv = temp1(1, 0)
                    tv2 = temp1(1, 1)
                    temp1(1, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    temp1(1, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(0, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(1, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(1, 0)
                    temp1(1, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(1, 1)
                    temp1(1, 1) = tv2
                End If

                If phasetype = "L" Then
                    Z = temp1(0, 0)
                    If temp1(0, 1) <> 0 Then
                        Z = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            Z = temp1(2, 0)
                        End If
                    End If
                ElseIf phasetype = "V" Then
                    Z = temp1(2, 0)
                    If temp1(2, 1) <> 0 Then
                        Z = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            Z = temp1(0, 0)
                        End If
                    End If
                End If
            Catch

                Dim findZ

                If phasetype = "V" Then

                    Z = 1
                    Do
                        findZ = coeff(3) * Z ^ 3 + coeff(2) * Z ^ 2 + coeff(1) * Z + coeff(0)
                        Z -= 0.00001
                        If Z < 0 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_ZError"))
                    Loop Until Math.Abs(findZ) < 0.0001

                Else

                    Z = 0
                    Do
                        findZ = coeff(3) * Z ^ 3 + coeff(2) * Z ^ 2 + coeff(1) * Z + coeff(0)
                        Z += 0.00001
                        If Z > 1 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_ZError"))
                    Loop Until Math.Abs(findZ) < 0.0001

                End If

            End Try

            Dim V = (Z * R * T / P) ' m3/mol

            Dim aux1 = -R / 2 * (0.45724 / T) ^ 0.5
            i = 0
            Dim aux2 = 0.0#
            Do
                j = 0
                Do
                    aux2 += Vz(i) * Vz(j) * (1 - VKij(i, j)) * (ci(j) * (ai(i) * Tc(j) / Pc(j)) ^ 0.5 + ci(i) * (ai(j) * Tc(i) / Pc(i)) ^ 0.5)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            dadT = aux1 * aux2

            Dim uu, ww As Double
            uu = 2
            ww = -1

            Dim DAres = am / (bm * (uu ^ 2 - 4 * ww) ^ 0.5) * Math.Log((2 * Z + BG1 * (uu - (uu ^ 2 - 4 * ww) ^ 0.5)) / (2 * Z + BG1 * (uu + (uu ^ 2 - 4 * ww) ^ 0.5))) - R * T * Math.Log((Z - BG1) / Z) - R * T * Math.Log(V * 101325 / (R * 298.15))
            Dim V0 As Double = R * 298.15 / 101325
            Dim DSres = R * Math.Log((Z - BG1) / Z) + R * Math.Log(V / V0) - 1 / (8 ^ 0.5 * bm) * dadT * Math.Log((2 * Z + BG1 * (2 - 8 ^ 0.5)) / (2 * Z + BG1 * (2 + 8 ^ 0.5)))
            Dim DHres = DAres + T * (DSres) + R * T * (Z - 1)

            Return Hid + DHres

        End Function

        Public Overrides Function CalcEntropy(ByVal phasetype As String, ByVal T As Double, ByVal P As Double, ByVal Vz As System.Array, ByVal VKij As Object, ByVal VTc As System.Array, ByVal VPc As System.Array, ByVal Vw As System.Array, ByVal Sid As Double, Optional ByVal otherargs As Object = Nothing) As Double

            Dim ai(), bi(), ci() As Double
            Dim n, R As Double
            Dim Tc(), Pc(), Vc(), w(), Zc(), alpha(), m(), a(,), b(,), Z, Tr() As Double
            Dim i, j, dadT

            n = UBound(Vz)

            ReDim ai(n), bi(n), ci(n), a(n, n), b(n, n)
            ReDim Tc(n), Pc(n), Vc(n), Zc(n), w(n), alpha(n), m(n), Tr(n)

            R = 8.314

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                w(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                alpha(i) = (1 + (0.37464 + 1.54226 * w(i) - 0.26992 * w(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                ci(i) = 0.37464 + 1.54226 * w(i) - 0.26992 * w(i) ^ 2
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim am = 0.0#
            Do
                j = 0
                Do
                    am = am + Vz(i) * Vz(j) * a(i, j)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bm = 0.0#
            Do
                bm = bm + Vz(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            Dim AG1 = am * P / (R * T) ^ 2
            Dim BG1 = bm * P / (R * T)

            Dim coeff(3) As Double

            coeff(0) = -AG1 * BG1 + BG1 ^ 2 + BG1 ^ 3
            coeff(1) = AG1 - 3 * BG1 ^ 2 - 2 * BG1
            coeff(2) = BG1 - 1
            coeff(3) = 1

            Dim temp1 = Poly_Roots(coeff)
            Dim tv
            Dim tv2
            Try

                If temp1(0, 0) > temp1(1, 0) Then
                    tv = temp1(1, 0)
                    tv2 = temp1(1, 1)
                    temp1(1, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    temp1(1, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(0, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(1, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(1, 0)
                    temp1(1, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(1, 1)
                    temp1(1, 1) = tv2
                End If

                If phasetype = "L" Then
                    Z = temp1(0, 0)
                    If temp1(0, 1) <> 0 Then
                        Z = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            Z = temp1(2, 0)
                        End If
                    End If
                ElseIf phasetype = "V" Then
                    Z = temp1(2, 0)
                    If temp1(2, 1) <> 0 Then
                        Z = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            Z = temp1(0, 0)
                        End If
                    End If
                End If
            Catch

                Dim findZ

                If phasetype = "V" Then

                    Z = 1
                    Do
                        findZ = coeff(3) * Z ^ 3 + coeff(2) * Z ^ 2 + coeff(1) * Z + coeff(0)
                        Z -= 0.00001
                        If Z < 0 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_ZError"))
                    Loop Until Math.Abs(findZ) < 0.0001

                Else

                    Z = 0
                    Do
                        findZ = coeff(3) * Z ^ 3 + coeff(2) * Z ^ 2 + coeff(1) * Z + coeff(0)
                        Z += 0.00001
                        If Z > 1 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_ZError"))
                    Loop Until Math.Abs(findZ) < 0.0001

                End If

            End Try

            Dim V = (Z * R * T / P) ' m3/mol

            Dim aux1 = -R / 2 * (0.45724 / T) ^ 0.5
            i = 0
            Dim aux2 = 0.0#
            Do
                j = 0
                Do
                    aux2 += Vz(i) * Vz(j) * (1 - VKij(i, j)) * (ci(j) * (ai(i) * Tc(j) / Pc(j)) ^ 0.5 + ci(i) * (ai(j) * Tc(i) / Pc(i)) ^ 0.5)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            dadT = aux1 * aux2

            Dim uu, ww As Double
            uu = 2
            ww = -1

            Dim DAres = am / (bm * (uu ^ 2 - 4 * ww) ^ 0.5) * Math.Log((2 * Z + BG1 * (uu - (uu ^ 2 - 4 * ww) ^ 0.5)) / (2 * Z + BG1 * (uu + (uu ^ 2 - 4 * ww) ^ 0.5))) - R * T * Math.Log((Z - BG1) / Z) - R * T * Math.Log(V * 101325 / (R * 298.15))
            Dim V0 As Double = R * 298.15 / 101325
            Dim DSres = R * Math.Log((Z - BG1) / Z) + R * Math.Log(V / V0) - 1 / (8 ^ 0.5 * bm) * dadT * Math.Log((2 * Z + BG1 * (2 - 8 ^ 0.5)) / (2 * Z + BG1 * (2 + 8 ^ 0.5)))
            Dim DHres = DAres + T * (DSres) + R * T * (Z - 1)

            Return Sid + DSres

        End Function

        Public Overrides Function CalcGibbsEnergy(ByVal phasetype As String, ByVal T As Double, ByVal P As Double, ByVal Vz As System.Array, ByVal VKij As Object, ByVal VTc As System.Array, ByVal VPc As System.Array, ByVal Vw As System.Array, ByVal Gid As Double, Optional ByVal otherargs As Object = Nothing) As Double

            Dim ai(), bi(), ci() As Double
            Dim n, R As Double
            Dim Tc(), Pc(), Vc(), w(), Zc(), alpha(), m(), a(,), b(,), Z, Tr() As Double
            Dim i, j, dadT

            n = UBound(Vz)

            ReDim ai(n), bi(n), ci(n), a(n, n), b(n, n)
            ReDim Tc(n), Pc(n), Vc(n), Zc(n), w(n), alpha(n), m(n), Tr(n)

            R = 8.314

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                w(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                alpha(i) = (1 + (0.37464 + 1.54226 * w(i) - 0.26992 * w(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                ci(i) = 0.37464 + 1.54226 * w(i) - 0.26992 * w(i) ^ 2
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim am = 0.0#
            Do
                j = 0
                Do
                    am = am + Vz(i) * Vz(j) * a(i, j)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bm = 0.0#
            Do
                bm = bm + Vz(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            Dim AG1 = am * P / (R * T) ^ 2
            Dim BG1 = bm * P / (R * T)

            Dim coeff(3) As Double

            coeff(0) = -AG1 * BG1 + BG1 ^ 2 + BG1 ^ 3
            coeff(1) = AG1 - 3 * BG1 ^ 2 - 2 * BG1
            coeff(2) = BG1 - 1
            coeff(3) = 1

            Dim temp1 = Poly_Roots(coeff)
            Dim tv
            Dim tv2
            Try

                If temp1(0, 0) > temp1(1, 0) Then
                    tv = temp1(1, 0)
                    tv2 = temp1(1, 1)
                    temp1(1, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    temp1(1, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(0, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(1, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(1, 0)
                    temp1(1, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(1, 1)
                    temp1(1, 1) = tv2
                End If

                If phasetype = "L" Then
                    Z = temp1(0, 0)
                    If temp1(0, 1) <> 0 Then
                        Z = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            Z = temp1(2, 0)
                        End If
                    End If
                ElseIf phasetype = "V" Then
                    Z = temp1(2, 0)
                    If temp1(2, 1) <> 0 Then
                        Z = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            Z = temp1(0, 0)
                        End If
                    End If
                End If
            Catch

                Dim findZ

                If phasetype = "V" Then

                    Z = 1
                    Do
                        findZ = coeff(3) * Z ^ 3 + coeff(2) * Z ^ 2 + coeff(1) * Z + coeff(0)
                        Z -= 0.00001
                        If Z < 0 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_ZError"))
                    Loop Until Math.Abs(findZ) < 0.0001

                Else

                    Z = 0
                    Do
                        findZ = coeff(3) * Z ^ 3 + coeff(2) * Z ^ 2 + coeff(1) * Z + coeff(0)
                        Z += 0.00001
                        If Z > 1 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_ZError"))
                    Loop Until Math.Abs(findZ) < 0.0001

                End If

            End Try

            Dim V = (Z * R * T / P) ' m3/mol

            Dim aux1 = -R / 2 * (0.45724 / T) ^ 0.5
            i = 0
            Dim aux2 = 0.0#
            Do
                j = 0
                Do
                    aux2 += Vz(i) * Vz(j) * (1 - VKij(i, j)) * (ci(j) * (ai(i) * Tc(j) / Pc(j)) ^ 0.5 + ci(i) * (ai(j) * Tc(i) / Pc(i)) ^ 0.5)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            dadT = aux1 * aux2

            Dim uu, ww As Double
            uu = 2
            ww = -1

            Dim DAres = am / (bm * (uu ^ 2 - 4 * ww) ^ 0.5) * Math.Log((2 * Z + BG1 * (uu - (uu ^ 2 - 4 * ww) ^ 0.5)) / (2 * Z + BG1 * (uu + (uu ^ 2 - 4 * ww) ^ 0.5))) - R * T * Math.Log((Z - BG1) / Z) - R * T * Math.Log(V * 101325 / (R * 298.15))
            Dim V0 As Double = R * 298.15 / 101325
            Dim DSres = R * Math.Log((Z - BG1) / Z) + R * Math.Log(V / V0) - 1 / (8 ^ 0.5 * bm) * dadT * Math.Log((2 * Z + BG1 * (2 - 8 ^ 0.5)) / (2 * Z + BG1 * (2 + 8 ^ 0.5)))
            Dim DHres = DAres + T * (DSres) + R * T * (Z - 1)

            Return Gid + (DHres - T * DSres)

        End Function

        Public Overrides Function CalcHelmoltzEnergy(ByVal phasetype As String, ByVal T As Double, ByVal P As Double, ByVal Vz As System.Array, ByVal VKij As Object, ByVal VTc As System.Array, ByVal VPc As System.Array, ByVal Vw As System.Array, ByVal Aid As Double, Optional ByVal otherargs As Object = Nothing) As Double

            Dim ai(), bi(), ci() As Double
            Dim n, R As Double
            Dim Tc(), Pc(), Vc(), w(), Zc(), alpha(), m(), a(,), b(,), Z, Tr() As Double
            Dim i, j, dadT

            n = UBound(Vz)

            ReDim ai(n), bi(n), ci(n), a(n, n), b(n, n)
            ReDim Tc(n), Pc(n), Vc(n), Zc(n), w(n), alpha(n), m(n), Tr(n)

            R = 8.314

            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                w(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                alpha(i) = (1 + (0.37464 + 1.54226 * w(i) - 0.26992 * w(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                ci(i) = 0.37464 + 1.54226 * w(i) - 0.26992 * w(i) ^ 2
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim am = 0.0#
            Do
                j = 0
                Do
                    am = am + Vz(i) * Vz(j) * a(i, j)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bm = 0.0#
            Do
                bm = bm + Vz(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            Dim AG1 = am * P / (R * T) ^ 2
            Dim BG1 = bm * P / (R * T)

            Dim coeff(3) As Double

            coeff(0) = -AG1 * BG1 + BG1 ^ 2 + BG1 ^ 3
            coeff(1) = AG1 - 3 * BG1 ^ 2 - 2 * BG1
            coeff(2) = BG1 - 1
            coeff(3) = 1

            Dim temp1 = Poly_Roots(coeff)
            Dim tv
            Dim tv2
            Try

                If temp1(0, 0) > temp1(1, 0) Then
                    tv = temp1(1, 0)
                    tv2 = temp1(1, 1)
                    temp1(1, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    temp1(1, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(0, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(0, 0)
                    temp1(0, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(0, 1)
                    temp1(0, 1) = tv2
                End If
                If temp1(1, 0) > temp1(2, 0) Then
                    tv = temp1(2, 0)
                    temp1(2, 0) = temp1(1, 0)
                    temp1(1, 0) = tv
                    tv2 = temp1(2, 1)
                    temp1(2, 1) = temp1(1, 1)
                    temp1(1, 1) = tv2
                End If

                If phasetype = "L" Then
                    Z = temp1(0, 0)
                    If temp1(0, 1) <> 0 Then
                        Z = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            Z = temp1(2, 0)
                        End If
                    End If
                ElseIf phasetype = "V" Then
                    Z = temp1(2, 0)
                    If temp1(2, 1) <> 0 Then
                        Z = temp1(1, 0)
                        If temp1(1, 1) <> 0 Then
                            Z = temp1(0, 0)
                        End If
                    End If
                End If
            Catch

                Dim findZ

                If phasetype = "V" Then

                    Z = 1
                    Do
                        findZ = coeff(3) * Z ^ 3 + coeff(2) * Z ^ 2 + coeff(1) * Z + coeff(0)
                        Z -= 0.00001
                        If Z < 0 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_ZError"))
                    Loop Until Math.Abs(findZ) < 0.0001

                Else

                    Z = 0
                    Do
                        findZ = coeff(3) * Z ^ 3 + coeff(2) * Z ^ 2 + coeff(1) * Z + coeff(0)
                        Z += 0.00001
                        If Z > 1 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_ZError"))
                    Loop Until Math.Abs(findZ) < 0.0001

                End If

            End Try

            Dim V = (Z * R * T / P) ' m3/mol

            Dim aux1 = -R / 2 * (0.45724 / T) ^ 0.5
            i = 0
            Dim aux2 = 0.0#
            Do
                j = 0
                Do
                    aux2 += Vz(i) * Vz(j) * (1 - VKij(i, j)) * (ci(j) * (ai(i) * Tc(j) / Pc(j)) ^ 0.5 + ci(i) * (ai(j) * Tc(i) / Pc(i)) ^ 0.5)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            dadT = aux1 * aux2

            Dim uu, ww As Double
            uu = 2
            ww = -1

            Dim DAres = am / (bm * (uu ^ 2 - 4 * ww) ^ 0.5) * Math.Log((2 * Z + BG1 * (uu - (uu ^ 2 - 4 * ww) ^ 0.5)) / (2 * Z + BG1 * (uu + (uu ^ 2 - 4 * ww) ^ 0.5))) - R * T * Math.Log((Z - BG1) / Z) - R * T * Math.Log(V * 101325 / (R * 298.15))
            Dim V0 As Double = R * 298.15 / 101325
            Dim DSres = R * Math.Log((Z - BG1) / Z) + R * Math.Log(V / V0) - 1 / (8 ^ 0.5 * bm) * dadT * Math.Log((2 * Z + BG1 * (2 - 8 ^ 0.5)) / (2 * Z + BG1 * (2 + 8 ^ 0.5)))
            Dim DHres = DAres + T * (DSres) + R * T * (Z - 1)

            Return Aid + DAres

        End Function

        Public Overrides Function CalcP(ByVal V As Double, ByVal T As Double, ByVal Vx As System.Array, ByVal VKij As Object, ByVal VTc As System.Array, ByVal VPc As System.Array, ByVal Vw As System.Array, Optional ByVal otherargs As Object = Nothing) As Object

            Dim ai(), bi() As Double
            Dim n, R, P, coeff(3), tmp() As Double
            Dim Tc(), Pc(), W(), alpha(), Vant(0, 4), m(), a(,), b(,), Tr() As Double

            n = UBound(Vx)

            ReDim ai(n), bi(n), tmp(n + 1), a(n, n), b(n, n)
            ReDim Tc(n), Pc(n), W(n), alpha(n), m(n), Tr(n)

            R = 8.314

            Dim i, j As Integer
            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                alpha(i) = (1 + (0.48 + 1.574 * W(i) - 0.176 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                ai(i) = 0.42748 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                bi(i) = 0.08664 * R * Tc(i) / Pc(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim aml = 0.0#
            Do
                j = 0
                Do
                    aml = aml + Vx(i) * Vx(j) * a(i, j)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Dim bml = 0.0#
            Do
                bml = bml + Vx(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            P = R * T / (V - bml) - aml / (V * (V + bml))

            Return P

        End Function

        Public Overrides Function CalcLnFugTV(ByVal T As Double, ByVal V As Double, ByVal Vx As System.Array, ByVal VKij As Object, ByVal VTc As System.Array, ByVal VPc As System.Array, ByVal Vw As System.Array, Optional ByVal otherargs As Object = Nothing, Optional ByVal forcephase As String = "") As Double()

            Dim P As Double = Me.CalcP(V, T, Vx, VKij, VTc, VPc, Vw, otherargs)

            If P < 0 Then P = -P

            Dim n, R, coeff(3) As Double
            Dim Vant(0, 4) As Double
            Dim criterioOK As Boolean = False
            Dim AG, BG, aml, bml As Double
            Dim t1, t2, t3, t4, t5 As Double

            n = UBound(Vx)

            Dim ai(n), bi(n), tmp(n + 1), a(n, n), b(n, n)
            Dim aml2(n), amv2(n), LN_CF(n), PHI(n) As Double
            Dim Tc(n), Pc(n), W(n), alpha(n), m(n), Tr(n)

            R = 8.314

            Dim i, j As Integer
            i = 0
            Do
                Tc(i) = VTc(i)
                Tr(i) = T / Tc(i)
                Pc(i) = VPc(i)
                W(i) = Vw(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                alpha(i) = (1 + (0.37464 + 1.54226 * W(i) - 0.26992 * W(i) ^ 2) * (1 - (T / Tc(i)) ^ 0.5)) ^ 2
                ai(i) = 0.45724 * alpha(i) * R ^ 2 * Tc(i) ^ 2 / Pc(i)
                bi(i) = 0.0778 * R * Tc(i) / Pc(i)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    a(i, j) = (ai(i) * ai(j)) ^ 0.5 * (1 - VKij(i, j))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                aml2(i) = 0
                i = i + 1
            Loop Until i = n + 1

            i = 0
            aml = 0
            Do
                j = 0
                Do
                    aml = aml + Vx(i) * Vx(j) * a(i, j)
                    aml2(i) = aml2(i) + Vx(j) * a(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            bml = 0
            Do
                bml = bml + Vx(i) * bi(i)
                i = i + 1
            Loop Until i = n + 1

            AG = aml * P / (R * T) ^ 2
            BG = bml * P / (R * T)

            Dim Z = P * V / (R * T)

            i = 0
            Do
                t1 = bi(i) * (Z - 1) / bml
                t2 = -Math.Log(Z - BG)
                t3 = AG * (2 * aml2(i) / aml - bi(i) / bml)
                t4 = Math.Log((Z + (1 + 2 ^ 0.5) * BG) / (Z + (1 - 2 ^ 0.5) * BG))
                t5 = 2 * 2 ^ 0.5 * BG
                LN_CF(i) = t1 + t2 - (t3 * t4 / t5)
                i = i + 1
            Loop Until i = n + 1

            Return LN_CF


        End Function

        ''' <summary>
        ''' This procedure checks if the compressibility factor is within the allowable region for the specified phase. 
        ''' If not, it generates a pseudo-root cabable of generate properties for the specified phase in order to keep 
        ''' the flash convergence process going forward.
        ''' </summary>
        ''' <param name="Z">The calculated compressibility factor, coming from the EOS</param>
        ''' <param name="a">EOS 'a' mixture parameter</param>
        ''' <param name="b">EOS 'b' mixture parameter</param>
        ''' <param name="P">Pressure in Pa</param>
        ''' <param name="T">Temperature in K</param>
        ''' <param name="phaselabel">'L' for Liquid, 'V' for Vapor.</param>
        ''' <returns>A vector containing the calculated compressibility factor and pressure, if required. 
        ''' If the given compressibility factor is within the allowable range, it is returned together with 
        ''' the specified pressure (no pseudoroot calculation is required).</returns>
        ''' <remarks>This procedure is based on the paper: 
        ''' Mathias, P. M., Boston, J. F. and Watanasiri, S. (1984), 
        ''' Effective utilization of equations of state for thermodynamic properties in process simulation. 
        ''' AIChE J., 30: 182–186. doi: 10.1002/aic.690300203</remarks>
        Public Shared Function CheckRoot(Z As Double, a As Double, b As Double, P As Double, T As Double, phaselabel As String) As Double()

            If a * b = 0.0# Then Return New Double() {Z, P}

            Dim rho, dPdrho, R, Pnew, P_, rho_, Tmc,
                Zcorr, rhomax, rhomin, rhomc, dPdrholim, C0, C1, rho2 As Double
            Dim i As Integer

            R = 8.314

            If Z < 0.0# Then Z = -Z

            Tmc = 0.20268 * a / (R * b)
            If T > Tmc Then T = Tmc * 0.9

            rho = P / (Z * R * T)

            dPdrholim = 0.1 * R * T

            'find rhomax

            Dim fx, dfdx As Double
            rhomax = rho
            i = 0
            Do
                fx = (1 + rhomax * b - 3 * rhomax ^ 2 * b ^ 2 + rhomax ^ 3 * b ^ 3) / (rhomax * R * T - rhomax ^ 2 * (a - 2 * b * R * T) + rhomax ^ 3 * (a * b - b ^ 2 * R * T))
                dfdx = (2 * a * rhomax * (b * rhomax - 1) ^ 2 * (b * rhomax + 1) - R * T * (-b ^ 2 * rhomax ^ 2 + 2 * b * rhomax + 1) ^ 2) / (rhomax ^ 2 * (a * rhomax * (1 - b * rhomax) + R * T * (b ^ 2 * rhomax ^ 2 - 2 * b * rhomax - 1)) ^ 2)
                rhomax = rhomax - 0.7 * fx / dfdx
                If rhomax < 0 Then rhomax = -rhomax
                i += 1
            Loop Until Math.Abs(fx) < 0.000001 Or i = 100

            'find rhomin

            rhomin = 0.1
            i = 0
            Do
                fx = (rhomin * R * T - rhomin ^ 2 * (a - 2 * b * R * T) + rhomin ^ 3 * (a * b - b ^ 2 * R * T)) / (1 + rhomin * b - 3 * rhomin ^ 2 * b ^ 2 + rhomin ^ 3 * b ^ 3)
                dfdx = (R * T * (-b ^ 2 * rhomin ^ 2 + 2 * b * rhomin + 1) ^ 2 - 2 * a * rhomin * (b * rhomin - 1) ^ 2 * (b * rhomin + 1)) / (b ^ 3 * rhomin ^ 3 - 3 * b ^ 2 * rhomin ^ 2 + b * rhomin + 1) ^ 2
                rhomin = rhomin - 0.7 * fx / dfdx
                If rhomin < 0 Then rhomin = -rhomin
                i += 1
            Loop Until Math.Abs(fx) < 0.000001 Or i = 100

            'find rhomc

            i = 0
            rhomc = (rhomax - rhomin) / 2
            Do
                fx = -(2 * (b * R * T * (b ^ 2 * rhomc ^ 2 - 2 * b * rhomc - 1) ^ 3 - a * (b * rhomc - 1) ^ 3 * (2 * b ^ 3 * rhomc ^ 3 + 3 * b ^ 2 * rhomc ^ 2 + 1))) / (b ^ 3 * rhomc ^ 3 - 3 * b ^ 2 * rhomc ^ 2 + b * rhomc + 1) ^ 3
                dfdx = (6 * b * (b * R * T * (-b ^ 2 * rhomc ^ 2 + 2 * b * rhomc + 1) ^ 4 - 2 * a * (b * rhomc - 1) ^ 4 * (b ^ 4 * rhomc ^ 4 + 2 * b ^ 3 * rhomc ^ 3 + 2 * b * rhomc - 1))) / (b ^ 3 * rhomc ^ 3 - 3 * b ^ 2 * rhomc ^ 2 + b * rhomc + 1) ^ 4
                rhomc = rhomc - 0.7 * fx / dfdx
                i += 1
            Loop Until Math.Abs(fx) < 0.000001 Or i = 100

            dPdrho = (R * T * (-b ^ 2 * rho ^ 2 + 2 * b * rho + 1) ^ 2 - 2 * a * rho * (b * rho - 1) ^ 2 * (b * rho + 1)) / (b ^ 3 * rho ^ 3 - 3 * b ^ 2 * rho ^ 2 + b * rho + 1) ^ 2

            If phaselabel = "L" Then
                If dPdrho > dPdrholim And rho > rhomc Then
                    Return New Double() {Z, P}
                End If
            Else
                If dPdrho > dPdrholim Then
                    Return New Double() {Z, P}
                End If
            End If

            If phaselabel = "L" Then

                'find rho*, P*

                i = 0
                rho_ = rhomc * 1.1
                Do
                    fx = -0.1 * R * T + (R * T * (-b ^ 2 * rho_ ^ 2 + 2 * b * rho_ + 1) ^ 2 - 2 * a * rho_ * (b * rho_ - 1) ^ 2 * (b * rho_ + 1)) / (b ^ 3 * rho_ ^ 3 - 3 * b ^ 2 * rho_ ^ 2 + b * rho_ + 1) ^ 2
                    dfdx = -(2 * (b * R * T * (b ^ 2 * rho_ ^ 2 - 2 * b * rho_ - 1) ^ 3 - a * (b * rho_ - 1) ^ 3 * (2 * b ^ 3 * rho_ ^ 3 + 3 * b ^ 2 * rho_ ^ 2 + 1))) / (b ^ 3 * rho_ ^ 3 - 3 * b ^ 2 * rho_ ^ 2 + b * rho_ + 1) ^ 3
                    rho_ = rho_ - fx / dfdx
                    If rho_ < rhomc Then rho_ = rhomc * 1.02
                    i += 1
                Loop Until Math.Abs(fx) < 0.000001 Or i = 100

                P_ = (rho_ * R * T - rho_ ^ 2 * (a - 2 * b * R * T) + rho_ ^ 3 * (a * b - b ^ 2 * R * T)) / (1 + rho_ * b - 3 * rho_ ^ 2 * b ^ 2 + rho_ ^ 3 * b ^ 3)

                C1 = 0.1 * R * T * (rho_ - 0.7 * rhomc)

                C0 = P_ - C1 * Math.Log(rho_ - 0.7 * rhomc)

                rho = 0.7 * rhomc + Math.Exp((P - C0) / C1)

                Pnew = (rho * R * T - rho ^ 2 * (a - 2 * b * R * T) + rho ^ 3 * (a * b - b ^ 2 * R * T)) / (1 + rho * b - 3 * rho ^ 2 * b ^ 2 + rho ^ 3 * b ^ 3)

                Zcorr = Pnew / (rho * R * T)

                If Double.IsNaN(Zcorr) Or Double.IsNaN(Pnew) Or Double.IsInfinity(Zcorr) Or Double.IsInfinity(Pnew) Then
                    Return New Double() {Z, P}
                Else
                    If Zcorr < 0.0# Or Pnew < 0.0# Then
                        Return New Double() {Z, P}
                    Else
                        Return New Double() {Zcorr, Pnew}
                    End If
                End If

            Else

                'find rho*, P*

                i = 0
                rho_ = rhomc * 0.9
                Do
                    fx = -0.1 * R * T + (R * T * (-b ^ 2 * rho_ ^ 2 + 2 * b * rho_ + 1) ^ 2 - 2 * a * rho_ * (b * rho_ - 1) ^ 2 * (b * rho_ + 1)) / (b ^ 3 * rho_ ^ 3 - 3 * b ^ 2 * rho_ ^ 2 + b * rho_ + 1) ^ 2
                    dfdx = -(2 * (b * R * T * (b ^ 2 * rho_ ^ 2 - 2 * b * rho_ - 1) ^ 3 - a * (b * rho_ - 1) ^ 3 * (2 * b ^ 3 * rho_ ^ 3 + 3 * b ^ 2 * rho_ ^ 2 + 1))) / (b ^ 3 * rho_ ^ 3 - 3 * b ^ 2 * rho_ ^ 2 + b * rho_ + 1) ^ 3
                    rho_ = rho_ - fx / dfdx
                    If rho_ < rhomc Then rho_ = rhomc * 0.98
                    i += 1
                Loop Until Math.Abs(fx) < 0.000001 Or i = 100

                P_ = (rho_ * R * T - rho_ ^ 2 * (a - 2 * b * R * T) + rho_ ^ 3 * (a * b - b ^ 2 * R * T)) / (1 + rho_ * b - 3 * rho_ ^ 2 * b ^ 2 + rho_ ^ 3 * b ^ 3)

                rho2 = (rho_ + rhomc) / 2

                C0 = -2 * rho_ * rho2 ^ 2 / (0.1 * R * T * (rho_ ^ 2 - rho2 ^ 2))

                C1 = 2 * rho_ / (0.1 * R * T * (rho_ ^ 2 - rho2 ^ 2))

                rho = (((1 / P) - C0) / C1) ^ 0.5

                Pnew = (rho * R * T - rho ^ 2 * (a - 2 * b * R * T) + rho ^ 3 * (a * b - b ^ 2 * R * T)) / (1 + rho * b - 3 * rho ^ 2 * b ^ 2 + rho ^ 3 * b ^ 3)

                Zcorr = Pnew / (rho * R * T)

                If Double.IsNaN(Zcorr) Or Double.IsNaN(Pnew) Or Double.IsInfinity(Zcorr) Or Double.IsInfinity(Pnew) Then
                    Return New Double() {Z, P}
                Else
                    If Zcorr < 0.0# Or Pnew < 0.0# Then
                        Return New Double() {Z, P}
                    Else
                        Return New Double() {Zcorr, Pnew}
                    End If
                End If

            End If

            'PR EOS P=f(rho) derivatives
            'P = (rho * R * T - rho ^ 2 * (a - 2 * b * R * T) + rho ^ 3 * (a * b - b ^ 2 * R * T)) / (1 + rho * b - 3 * rho ^ 2 * b ^ 2 + rho ^ 3 * b ^ 3)
            'dPdrho = (R * T * (-b ^ 2 * rho ^ 2 + 2 * b * rho + 1) ^ 2 - 2 * a * rho * (b * rho - 1) ^ 2 * (b * rho + 1)) / (b ^ 3 * rho ^ 3 - 3 * b ^ 2 * rho ^ 2 + b * rho + 1) ^ 2
            'd2Pdrho2 = -(2 * (b * R * T * (b ^ 2 * rho ^ 2 - 2 * b * rho - 1) ^ 3 - a * (b * rho - 1) ^ 3 * (2 * b ^ 3 * rho ^ 3 + 3 * b ^ 2 * rho ^ 2 + 1))) / (b ^ 3 * rho ^ 3 - 3 * b ^ 2 * rho ^ 2 + b * rho + 1) ^ 3
            'd3Pdrho3 = (6 * b * (b * R * T * (-b ^ 2 * rho ^ 2 + 2 * b * rho + 1) ^ 4 - 2 * a * (b * rho - 1) ^ 4 * (b ^ 4 * rho ^ 4 + 2 * b ^ 3 * rho ^ 3 + 2 * b * rho - 1))) / (b ^ 3 * rho ^ 3 - 3 * b ^ 2 * rho ^ 2 + b * rho + 1) ^ 4


        End Function

    End Class

End Namespace
