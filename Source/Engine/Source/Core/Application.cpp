#include "Application.h"
#include "iostream"

namespace Janus
{

#define BIND_EVENT_FN(fn) [this](auto &&...args) -> decltype(auto) { return this->fn(std::forward<decltype(args)>(args)...); }

	Application *Application::s_Instance = nullptr;

	Application::Application(const std::string& name)
	{
		m_Name = name;
	}

	Application::~Application()
	{
	}

	void Application::Run()
	{
		// MAIN APP LOOP
		while (m_Running)
		{
			std::cout << "Running application " << m_Name;
		}
	}

	void Application::Close()
	{
		m_Running = false;
	}
}