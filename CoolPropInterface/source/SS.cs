/* ----------------------------------------------------------------------------
 * This file was automatically generated by SWIG (http://www.swig.org).
 * Version 3.0.2
 *
 * Do not make changes to this file unless you know what you are doing--modify
 * the SWIG interface file instead.
 * ----------------------------------------------------------------------------- */


public class SS : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal SS(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(SS obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~SS() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          CoolPropPINVOKE.delete_SS(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public double rhomolar {
    set {
      CoolPropPINVOKE.SS_rhomolar_set(swigCPtr, value);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      double ret = CoolPropPINVOKE.SS_rhomolar_get(swigCPtr);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public double T {
    set {
      CoolPropPINVOKE.SS_T_set(swigCPtr, value);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      double ret = CoolPropPINVOKE.SS_T_get(swigCPtr);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public double p {
    set {
      CoolPropPINVOKE.SS_p_set(swigCPtr, value);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      double ret = CoolPropPINVOKE.SS_p_get(swigCPtr);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public double hmolar {
    set {
      CoolPropPINVOKE.SS_hmolar_set(swigCPtr, value);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      double ret = CoolPropPINVOKE.SS_hmolar_get(swigCPtr);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public double smolar {
    set {
      CoolPropPINVOKE.SS_smolar_set(swigCPtr, value);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      double ret = CoolPropPINVOKE.SS_smolar_get(swigCPtr);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public double umolar {
    set {
      CoolPropPINVOKE.SS_umolar_set(swigCPtr, value);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      double ret = CoolPropPINVOKE.SS_umolar_get(swigCPtr);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public double Q {
    set {
      CoolPropPINVOKE.SS_Q_set(swigCPtr, value);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
    } 
    get {
      double ret = CoolPropPINVOKE.SS_Q_get(swigCPtr);
      if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
      return ret;
    } 
  }

  public SS() : this(CoolPropPINVOKE.new_SS(), true) {
    if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
  }

  public bool is_valid() {
    bool ret = CoolPropPINVOKE.SS_is_valid(swigCPtr);
    if (CoolPropPINVOKE.SWIGPendingException.Pending) throw CoolPropPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

}
