using static SDL2.SDL;
using Magician.Library;
using Magician.Renderer;
using Silk.NET.OpenGL;

namespace Magician;

class MagicianSDL
{
    static IntPtr win;
    bool done = false;
    int frames = 0;
    int stopFrame = -1;
    int driveDelay = 0;
    double timeResolution = 0.1;

    static void Main(string[] args)
    {
        /* Startup */
        Console.WriteLine(Data.App.Title);
        MagicianSDL magicianSDL = new MagicianSDL();

        magicianSDL.InitSDL();
        magicianSDL.CreateWindow();
        //magicianSDL.CreateRenderer();
        SDL2.SDL_ttf.TTF_Init();
        Renderer.Text.FallbackFontPath = "magician/ui/assets/fonts/Space_Mono/SpaceMono-Regular.ttf";

        // Create a Silk.Net context
        Renderer.RGlobals.sdlContext = new(win, null,
            (SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 3),
            (SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 3),
            (SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE)
        );
        Renderer.RGlobals.sdlContext.MakeCurrent();
        Renderer.RGlobals.gl = new GL(Renderer.RGlobals.sdlContext);
        Renderer.RGlobals.gl.Enable(Silk.NET.OpenGL.GLEnum.DepthTest);

        // Generate shaders
        // TODO: move this
        RDrawable.GenShaders();

        // Load a spell
        Spellcaster.Prepare(new Demos.DefaultSpell());
        Spellcaster.Cast();

        // Run
        magicianSDL.MainLoop();

        // Cleanup
        //SDL_DestroyRenderer(SDLGlobals.renderer);
        SDL_DestroyWindow(win);
        SDL_Quit();
    }

    void MainLoop()
    {
        // Create a texture from the surface
        // Textures are hardware-acclerated, while surfaces use CPU rendering
        //SDLGlobals.renderedTexture = SDL_CreateTexture(SDLGlobals.renderer, SDL_PIXELFORMAT_RGBA8888, (int)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Data.Globals.winWidth, Data.Globals.winHeight);

        while (!done)
        {
            Spellcaster.Animate(frames * timeResolution);

            // Event handling
            while (SDL_PollEvent(out SDL_Event sdlEvent) != 0 ? true : false)
            {
                Interactive.Events.Process(sdlEvent);
                switch (sdlEvent.type)
                {
                    case SDL_EventType.SDL_WINDOWEVENT:
                        SDL_WindowEvent windowEvent = sdlEvent.window;
                        switch (windowEvent.windowEvent)
                        {

                            case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                Data.Globals.winWidth = windowEvent.data1;
                                Data.Globals.winHeight = windowEvent.data2;
                                //SDLGlobals.renderedTexture = SDL_CreateTexture(SDLGlobals.renderer, SDL_PIXELFORMAT_RGBA8888, (int)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Data.Globals.winWidth, Data.Globals.winHeight);
                                break;
                        }
                        break;
                    case SDL_EventType.SDL_QUIT:
                        done = true;
                        break;
                }
            }

            // Draw things
            if (frames != stopFrame)
            {
                Render();
            }
        }
    }

    // Renders each frame to a texture and displays the texture
    void Render()
    {
        if (Renderer.RControl.doRender)
        {
            // Options
            //SDL_SetRenderDrawBlendMode(SDLGlobals.renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);
            //SDL_SetTextureBlendMode(SDLGlobals.renderedTexture, SDL_BlendMode.SDL_BLENDMODE_BLEND);

            // gl won't be null here, and if it is, it's not my fault
            RGlobals.gl!.Clear((uint)(Silk.NET.OpenGL.GLEnum.ColorBufferBit | Silk.NET.OpenGL.GLEnum.DepthBufferBit));

            // Draw objects
            Geo.Ref.Origin.Render(0, 0, 0);
            Renderer.RDrawable.DrawAll();
            Renderer.RDrawable.drawables.Clear();

            // SAVE FRAME TO IMAGE
            //if (Renderer.RControl.saveFrame && frames != stopFrame)
            if (frames == 20)
            {
                float[] pixels = new float[(int)(Data.Globals.winWidth * Data.Globals.winHeight)];
                unsafe
                {
                    Scribe.Info(Data.Globals.winWidth * Data.Globals.winHeight);
                    
                    //RGlobals.gl.ReadPixels(0, 0, (uint)Data.Globals.winWidth, (uint)Data.Globals.winHeight, GLEnum.Rgb, GLEnum.Float, &pixels);
                    //Scribe.Info<float>(pixels);
                }
            }

            if (Renderer.RControl.display)
            {
                SDL_GL_SwapWindow(win);
            }
        }
        frames++;
    }

    void InitSDL()
    {
        if (SDL_Init(SDL_INIT_VIDEO) < 0)
        {
            Console.WriteLine($"Error initializing SDL: {SDL_GetError()}");
        }
    }
    void CreateWindow()
    {
        win = SDL_CreateWindow(Data.App.Title, 0, 0, (int)Data.Globals.winWidth, (int)Data.Globals.winHeight, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

        if (win == IntPtr.Zero)
        {
            Console.WriteLine($"Error creating the window: {SDL_GetError()}");
        }
    }
}