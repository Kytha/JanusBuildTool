#include "Engine/Source/Core/Application.h"
#include "Engine/Source/Core/EntryPoint.h"
class Game : public Janus::Application
{
    public:
        Game() : Application("My Cool Game")
        {
            
        }

        ~Game()
        {

        }
};

Janus::Application* Janus::CreateApplication()
{
    return new Game;
} 