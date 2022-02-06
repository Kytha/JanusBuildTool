extern Janus::Application* Janus::CreateApplication();
// ENTRY POINT
int main(int argc, char** argv)
{
	auto app = Janus::CreateApplication();
	app->Run();
	delete app;
}