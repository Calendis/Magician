namespace Magician;
using Paint;
using Core.Caster;
using static Core.Caster.Spellbook;
using Silk.NET.OpenGL;
using static SDL2.SDL;
using Silk.NET.Maths;

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
        Console.WriteLine(Runes.App.Title);
        MagicianSDL magicianSDL = new MagicianSDL();

        magicianSDL.InitSDL();
        magicianSDL.CreateWindow();
        SDL2.SDL_ttf.TTF_Init();
        Paint.Text.FallbackFontPath = "magician/ui/assets/fonts/Space_Mono/SpaceMono-Regular.ttf";

        // Create a Silk.Net context
        Renderer.SDL = new(win, null,
            (SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 3),
            (SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 3),
            (SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE)
        );
        Renderer.SDL.MakeCurrent();
        Renderer.GL = new GL(Renderer.SDL);
        Renderer.GL.Enable(GLEnum.DepthTest);
        Renderer.Display = true;
        Renderer.Render = true;
        //Renderer.SDL.SwapInterval(0);

        // Generate shaders
        // TODO: move this
        Shaders.Generate();

        // Load a spell
        Prepare(new Demos.DefaultSpell());
        Cast();

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
            Animate(frames * timeResolution);

            // Event handling
            while (SDL_PollEvent(out SDL_Event sdlEvent) != 0)
            {
                Interactive.Events.Process(sdlEvent);
                switch (sdlEvent.type)
                {
                    case SDL_EventType.SDL_WINDOWEVENT:
                        SDL_WindowEvent windowEvent = sdlEvent.window;
                        switch (windowEvent.windowEvent)
                        {

                            case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                Runes.Globals.winWidth = windowEvent.data1;
                                Runes.Globals.winHeight = windowEvent.data2;
                                Renderer.GL.Viewport(new Vector2D<int>((int)Runes.Globals.winWidth, (int)Runes.Globals.winHeight));
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
        if (Renderer.Render)
        {
            // gl won't be null here, and if it is, it's not my fault
            Renderer.GL.Clear((uint)(GLEnum.ColorBufferBit | GLEnum.DepthBufferBit));

            // Draw objects
            Geo.Ref.Origin.Render(0, 0, 0);
            Renderer.DrawAll();
            Renderer.Drawables.Clear();

            // SAVE FRAME TO IMAGE
            //if (Renderer.RControl.saveFrame && frames != stopFrame)
        }
        if (Renderer.Display)
        {
            SDL_GL_SwapWindow(win);
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
        win = SDL_CreateWindow(Runes.App.Title, 0, 0, (int)Runes.Globals.winWidth, (int)Runes.Globals.winHeight, SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL_WindowFlags.SDL_WINDOW_OPENGL);

        if (win == IntPtr.Zero)
        {
            Console.WriteLine($"Error creating the window: {SDL_GetError()}");
        }
    }
}