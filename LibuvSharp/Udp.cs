using System;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Udp : Handle
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void recv_start_callback_win(IntPtr handle, IntPtr nread, WindowsBufferStruct buf, IntPtr sockaddr, ushort flags);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void recv_start_callback_unix(IntPtr handle, IntPtr nread, UnixBufferStruct buf, IntPtr sockaddr, ushort flags);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_init(IntPtr loop, IntPtr handle);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_bind(IntPtr handle, sockaddr_in sockaddr, short flags);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_bind6(IntPtr handle, sockaddr_in6 sockaddr, short flags);

		recv_start_callback_win recv_start_cb_win;
		recv_start_callback_unix recv_start_cb_unix;

		public Udp()
			: this(Loop.Default)
		{
		}

		public Udp(Loop loop)
			: base(loop, UvHandleType.UV_UDP)
		{
			int r = uv_udp_init(loop.Handle, handle);
			Ensure.Success(r, loop);
			// we can't supply just recv_start_callback in Receive
			// because it will create a temporary delegate which could(and will) be garbage collected at any time
			// happens in my case after 10 or 20 calls
			// so we have to reference it, so it won't garbage collect it until the object itself
			// is gone
			recv_start_cb_win = recv_start_callback_w;
			recv_start_cb_unix = recv_start_callback_u;
		}

		public void Bind(IPAddress ipAddress, int port)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");

			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_udp_bind(handle, UV.uv_ip4_addr(ipAddress.ToString(), port), 0);
			} else {
				r = uv_udp_bind6(handle, UV.uv_ip6_addr(ipAddress.ToString(), port), 0);
			}
			Ensure.Success(r, Loop);
		}
		public void Bind(string ipAddress, int port)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Bind(IPAddress.Parse(ipAddress), port);
		}
		public void Bind(IPEndPoint endPoint)
		{
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			Bind(endPoint.Address, endPoint.Port);
		}

		[DllImport("uv", EntryPoint = "uv_udp_send", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_send_win(IntPtr req, IntPtr handle, WindowsBufferStruct[] bufs, int bufcnt, sockaddr_in addr, callback callback);
		[DllImport("uv", EntryPoint = "uv_udp_send", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_send_unix(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int bufcnt, sockaddr_in addr, callback callback);

		[DllImport("uv", EntryPoint = "uv_udp_send6", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_send6_win(IntPtr req, IntPtr handle, WindowsBufferStruct[] bufs, int bufcnt, sockaddr_in6 addr, callback callback);
		[DllImport("uv", EntryPoint = "uv_udp_send6", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_send6_unix(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int bufcnt, sockaddr_in6 addr, callback callback);

		public void Send(IPAddress ipAddress, int port, byte[] data, int length, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Ensure.ArgumentNotNull(data, "data");
			Ensure.ArgumentNotNull(data, "callback");

			GCHandle datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			CallbackPermaRequest cpr = new CallbackPermaRequest(UvRequestType.UV_UDP_SEND);
			cpr.Callback += (status, cpr2) => {
				datagchandle.Free();
				if (callback != null) {
					callback(status == 0);
				}
			};


			int r;
			if (UV.isUnix) {
				UnixBufferStruct[] buf = new UnixBufferStruct[1];
				buf[0] = new UnixBufferStruct(datagchandle.AddrOfPinnedObject(), length);

				if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
					r = uv_udp_send_unix(cpr.Handle, handle, buf, 1, UV.uv_ip4_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
				} else {
					r = uv_udp_send6_unix(cpr.Handle, handle, buf, 1, UV.uv_ip6_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
				}
			} else {
				WindowsBufferStruct[] buf = new WindowsBufferStruct[1];
				buf[0] = new WindowsBufferStruct(datagchandle.AddrOfPinnedObject(), length);

				if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
					r = uv_udp_send_win(cpr.Handle, handle, buf, 1, UV.uv_ip4_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
				} else {
					r = uv_udp_send6_win(cpr.Handle, handle, buf, 1, UV.uv_ip6_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
				}
			}
			Ensure.Success(r, Loop);
		}
		public void Send(IPAddress ipAddress, int port, byte[] data, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(ipAddress, port, data, data.Length, callback);
		}
		public void Send(IPAddress ipAddress, int port, byte[] data, int length)
		{
			Send(ipAddress, port, data, length, null);
		}
		public void Send(IPAddress ipAddress, int port, byte[] data)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(ipAddress, port, data, data.Length);
		}

		public void Send(string ipAddress, int port, byte[] data, int length, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Send(IPAddress.Parse(ipAddress), port, data, length, callback);
		}
		public void Send(string ipAddress, int port, byte[] data, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(ipAddress, port, data, data.Length, callback);
		}
		public void Send(string ipAddress, int port, byte[] data, int length)
		{
			Send(ipAddress, port, data, length, null);
		}
		public void Send(string ipAddress, int port, byte[] data)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(ipAddress, port, data, data.Length);
		}

		public void Send(IPEndPoint endPoint, byte[] data, int length, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			Send(endPoint.Address, endPoint.Port, data, length, callback);
		}
		public void Send(IPEndPoint endPoint, byte[] data, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(endPoint, data, data.Length, callback);
		}
		public void Send(IPEndPoint endPoint, byte[] data, int length)
		{
			Send(endPoint, data, length, null);
		}
		public void Send(IPEndPoint endPoint, byte[] data)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(endPoint, data, data.Length);
		}

		[DllImport("uv", EntryPoint = "uv_udp_recv_start", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_recv_start_win(IntPtr handle, alloc_callback_win alloc_callback, recv_start_callback_win callback);

		[DllImport("uv", EntryPoint = "uv_udp_recv_start", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_recv_start_unix(IntPtr handle, alloc_callback_unix alloc_callback, recv_start_callback_unix callback);

		internal void recv_start_callback_w(IntPtr handle, IntPtr nread, WindowsBufferStruct buf, IntPtr sockaddr, ushort flags)
		{
			recv_start_callback(handle, nread, sockaddr, flags);
		}
		internal void recv_start_callback_u(IntPtr handle, IntPtr nread, UnixBufferStruct buf, IntPtr sockaddr, ushort flags)
		{
			recv_start_callback(handle, nread, sockaddr, flags);
		}
		internal void recv_start_callback(IntPtr handle, IntPtr nread, IntPtr sockaddr, ushort flags)
		{
			int n = (int)nread;

			if (n == 0) {
				return;
			}

			OnMessage(UV.GetIPEndPoint(sockaddr), Loop.buffer.Get(n));
		}

		bool receive_init = false;
		public void Receive(Action<IPEndPoint, byte[]> callback)
		{
			Ensure.ArgumentNotNull(callback, "callback");

			if (!receive_init) {
				int r;
				if (UV.isUnix) {
					r = uv_udp_recv_start_unix(handle, Loop.buffer.AllocCallbackUnix, recv_start_cb_unix);
				} else {
					r = uv_udp_recv_start_win(handle, Loop.buffer.AllocCallbackWin, recv_start_cb_win);
				}
				Ensure.Success(r, Loop);
				receive_init = true;
			}
			Message += callback;
		}

		public void Receive(Encoding encoding, Action<IPEndPoint, string> callback)
		{
			Ensure.ArgumentNotNull(encoding, "encoding");
			Ensure.ArgumentNotNull(callback, "callback");

			Receive((ep, data) => callback(ep, encoding.GetString(data)));
		}

		event Action<IPEndPoint, byte[]> Message = null;
		void OnMessage(IPEndPoint endPoint, byte[] data)
		{
			if (Message != null) {
				Message(endPoint, data);
			}
		}
	}
}

