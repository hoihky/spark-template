#include "game/ArpgGame.hpp"
#include "spark/engine/Engine.hpp"

int main() {
    Spark::Engine engine(Spark::Engine::NewGame<Arpg::ArpgGame>());
    engine.Run();
    return 0;
}
