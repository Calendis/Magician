// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/* Modified for Magician for use with SDL2 C# bindings from https://github.com/flibitijibibo/SDL2-CS */

using System;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Loader;
using Silk.NET.Maths;
//using Silk.NET.SDL;
using static SDL2.SDL;

namespace Magician.Paint;
public unsafe class SdlContext : IGLContext
{
    //private readonly Sdl _sdl;
    private IntPtr _ctx;
    private IntPtr _window;

    /// <summary>
    /// Creates a <see cref="SdlContext"/> from a native window using the given native interface.
    /// </summary>
    /// <param name="sdl">The native interface to use.</param>
    /// <param name="window">The native window to associate this context for.</param>
    /// <param name="source">The <see cref="IGLContextSource" /> to associate this context to, if any.</param>
    /// <param name="attributes">The attributes to eagerly pass to <see cref="Create"/>.</param>
    public SdlContext(//Sdl sdl,
        IntPtr window,
        IGLContextSource? source = null,
        params (SDL_GLattr Attribute, int Value)[] attributes)
    {
        //_sdl = sdl;
        Window = window;
        Source = source;
        if (attributes is not null && attributes.Length > 0)
        {
            Create(attributes);
        }
    }

    /// <summary>
    /// The native window to create a context for.
    /// </summary>
    public IntPtr Window
    {
        get => _window;
        set
        {
            AssertNotCreated();
            _window = value;
        }
    }

    /// <inheritdoc cref="IGLContext" />
    public Vector2D<int> FramebufferSize
    {
        get
        {
            AssertCreated();
            var ret = stackalloc int[2];
            SDL_GL_GetDrawableSize(Window, out ret[0], out ret[1]);
            //_sdl.ThrowError();
            return *(Vector2D<int>*)ret;
        }
    }

    /// <inheritdoc cref="IGLContext" />
    public void Create(params (SDL_GLattr Attribute, int Value)[] attributes)
    {
        foreach (var (attribute, value) in attributes)
        {
            if (SDL_GL_SetAttribute(attribute, value) != 0)
            {
                //_sdl.ThrowError();
            }
        }

        _ctx = SDL_GL_CreateContext(Window);
        if (_ctx == IntPtr.Zero)
        {
            //_sdl.ThrowError();
        }
    }

    private void AssertCreated()
    {
        if (_ctx == IntPtr.Zero)
        {
            throw new InvalidOperationException("Context not created.");
        }
    }

    private void AssertNotCreated()
    {
        if (_ctx != IntPtr.Zero)
        {
            throw new InvalidOperationException("Context created already.");
        }
    }

    /// <inheritdoc cref="IGLContext" />
    public void Dispose()
    {
        if (_ctx != IntPtr.Zero)
        {
            SDL_GL_DeleteContext(_ctx);
            _ctx = IntPtr.Zero;
        }
    }

    /// <inheritdoc cref="IGLContext" />
    public nint GetProcAddress(string proc, int? slot = default)
    {
        AssertCreated();
        SDL_ClearError();
        var ret = (nint)SDL_GL_GetProcAddress(proc);
        //_sdl.ThrowError();
        if (ret == 0)
        {
            Throw(proc);
            return 0;
        }

        return ret;
        static void Throw(string proc) => throw new SymbolLoadingException(proc);
    }

    public bool TryGetProcAddress(string proc, out nint addr, int? slot = default)
    {
        addr = 0;
        SDL_ClearError();
        if (_ctx == IntPtr.Zero)
        {
            return false;
        }

        var ret = (nint)SDL_GL_GetProcAddress(proc);
        if (!string.IsNullOrWhiteSpace(SDL_GetError()))
        {
            SDL_ClearError();
            return false;
        }

        return (addr = ret) != 0;
    }

    /// <inheritdoc cref="IGLContext" />
    public nint Handle
    {
        get
        {
            AssertCreated();
            return (nint)_ctx;
        }
    }

    /// <inheritdoc cref="IGLContext" />
    public IGLContextSource? Source { get; }

    /// <inheritdoc cref="IGLContext" />
    public bool IsCurrent
    {
        get
        {
            AssertCreated();
            return SDL_GL_GetCurrentContext() == _ctx;
        }
    }

    /// <inheritdoc cref="IGLContext" />
    public void SwapInterval(int interval)
    {
        AssertCreated();
        SDL_GL_SetSwapInterval(interval);
    }

    /// <inheritdoc cref="IGLContext" />
    public void SwapBuffers()
    {
        AssertCreated();
        SDL_GL_SwapWindow(Window);
    }

    /// <inheritdoc cref="IGLContext" />
    public void MakeCurrent()
    {
        AssertCreated();
        SDL_GL_MakeCurrent(Window, _ctx);
    }

    /// <inheritdoc cref="IGLContext" />
    public void Clear()
    {
        AssertCreated();
        if (IsCurrent)
        {
            SDL_GL_MakeCurrent(Window, IntPtr.Zero);
        }
    }
}