using System;

namespace LibuvSharp
{
	internal enum uv_err_code
	{
		UV_UNKNOWN = -1,
		UV_OK = 0,
		UV_EOF = 1,
		UV_EADDRINFO = 2,
		UV_EACCES = 3,
		UV_EAGAIN = 4,
		UV_EADDRINUSE = 5,
		UV_EADDRNOTAVAIL = 6,
		UV_EAFNOSUPPORT = 7,
		UV_EALREADY = 8,
		UV_EBADF = 9,
		UV_EBUSY = 10,
		UV_ECONNABORTED = 11,
		UV_ECONNREFUSED = 12,
		UV_ECONNRESET = 13,
		UV_EDESTADDRREQ = 14,
		UV_EFAULT = 15,
		UV_EHOSTUNREACH = 16,
		UV_EINTR = 17,
		UV_EINVAL = 18,
		UV_EISCONN = 19,
		UV_EMFILE = 20,
		UV_EMSGSIZE = 21,
		UV_ENETDOWN = 22,
		UV_ENETUNREACH = 23,
		UV_ENFILE = 24,
		UV_ENOBUFS = 25,
		UV_ENOMEM = 26,
		UV_ENOTDIR = 27,
		UV_EISDIR = 28,
		UV_ENONET = 29,
		UV_ENOTCONN = 31,
		UV_ENOTSOCK = 32,
		UV_ENOTSUP = 33,
		UV_ENOENT = 34,
		UV_ENOSYS = 35,
		UV_EPIPE = 36,
		UV_EPROTO = 37,
		UV_EPROTONOSUPPORT = 38,
		UV_EPROTOTYPE = 39,
		UV_ETIMEDOUT = 40,
		UV_ECHARSET = 41,
		UV_EAIFAMNOSUPPORT = 42,
		UV_EAISERVICE = 44,
		UV_EAISOCKTYPE = 45,
		UV_ESHUTDOWN = 46,
		UV_EEXIST = 47,
		UV_ESRCH = 48,
		UV_ENAMETOOLONG = 49,
		UV_EPERM = 50,
		UV_ELOOP = 51,
		UV_EXDEV = 52,
		UV_ENOTEMPTY = 53,
		UV_ENOSPC = 54,
		UV_EIO = 55,
		UV_EROFS = 56,
		UV_ENODEV = 57,
		UV_ESPIPE = 58,
		UV_ECANCELED = 59,
	}
}
