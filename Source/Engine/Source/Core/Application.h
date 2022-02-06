#pragma once
#include <string>

namespace Janus
{
	// Application is a singleton object which maintains the window and game loop
	class Application
	{

	public:
		Application(const std::string& name);
		virtual ~Application();

		void Run();
		void Close();
	private:
		// Pointer to the apps application object
        std::string m_Name;
        bool m_Running = true;
		static Application *s_Instance;
	};

	// To be defined in client
	// Client defines a Janus Application object to be returned by this function
	Application *CreateApplication();
}
