# Jogo por Turnos - Redes

## Autoria

- António Rodrigues, 22202884

## Introdução

Este jogo é um jogo de tiros jogado por turnos, numa grelha, em que cada jogador escolhe no seu turno se quer andar e disparar.
O jogo usa a package [Unity Netcode for GameObjects](https://docs.unity3d.com/Manual/com.unity.netcode.gameobjects.html) e a package [Relay](https://docs.unity3d.com/Manual/com.unity.services.relay.html) também do Unity.
Usa como base o código feito em aula com o Professor Diogo Andrade.

## Descrição Técnica

### Jogo

Ao abrir temos duas opções, iniciar um servidor ou entrar num como jogador:

![First screen](./Images/first_view.png)

Quando se clica para iniciar o servidor, aparece este ecrã, com o código para os jogador se conectarem ao lado:

![Server view](./Images/server_view_before_players.png)

Um jogador ao pôr esse código no campo por baixo do botão e ao clicar entra no jogo:

![Jogador conectado](./Images/player_connecting.png)

Vista lado a lado do servidor (esquerda) e do jogador (direita).
O jogador tem no seu UI o icon do personagem que está a controlar, e por baixo o movimento que decidiu tomar nessa ronda.
O servidor tem um botão para começar o jogo mais cedo e ignorar o temporizador de 30 segundos.
De resto vêm o mesmo.

![Side by side](./Images/side_by_side_player_server.png)

O jogo começa após 30 segundos depois de ter pelo menos 2 jogadores conectados, e permite um máximo de 4 jogadores.
Os jogadores escolhem o seu movimento com:
- W: Andar para cima;
- S: Andar para baixo;
- D: Andar para a direita;
- A: Andar para a esquerda;
- Espaço: Disparar na direção do último movimento escolhido.

Esses movimentos são dispostos por baixo do icon do jogador, com um icon da decisão, quando se despara a bala fica da cor da direção em que vai:

![During game](./Images/move_up.png)
![During game](./Images/move_right.png)
![During game](./Images/shoot_right.png)

Durante o jogo cada jogador tem 2 segundos para escolher a sua próxima ação e depois essa ação é executada.

### Implementação em Unity

Todo o jogo é controlado pelo servidor, e os jogadores comunicam com o servidor a sua ação.
No ínicio todos os jogos são iguais com esta configuração:
- NetworkManager: tem os componentes Network Manager, Unity Transport e Network Setup;
- SpawnPoints: tem 4 objetos vazios para dar como ponto inicial para cada jogador;
- StartMenu: tem os dois botões e o campo para o código;
- StartGame: caso seja o servidor, tem o botão de começar mais cedo;
- GameInterface: tem a grelha de jogo e o UI com o código;
- Loading: um ecrã que apenas diz "Loading..." enquanto o jogador se conecta ou o servidor inicia.

![Inspector](./Images/unity_inspector.png)

Dentro do NetworkManager temos o Network Setup que é onde tudo acontece:

Temos dois métodos iniciais, o StartServer e o StartPlayer, este métodos são chamados com os botões Start Server e Start Player, respetivamente.
Em ambos os métodos inicializamos uma lista de *Player* para guardar as sprites que são dispostas durante o jogo.
Depois verificamos se o prótocolo do UnityTransport é um relay server, e se for, tornamos a variável isRelay verdadeira.
No final desligamos a interface do menu que inclui os botões e ativamos a interface com a mensagem de "Loading...":

![Metodos](./Images/metodos_iniciais.png)

Depois temos o método StartAsServerCR, que é chamado no método StartServer, neste método conectamo-nos ao relay caso o booleano isRelay esteja em verdadeiro, e é ai que recebemos o código para os jogadores se conectarem.
Iniciamos o servidor, e caso o servidor seja iniciado instanciamos o objeto GameManager, que vai controlar todo o jogo e mexer os sprites dos jogadores, instanciar as balas, detetar colisões e controlar o tempo e os turnos.
Por fim desligamos a interface de Loading, e ativamos a interface com o botão de começar mais cedo, e a interface do jogo:

![Metodo Server](./Images/server_metodo.png)
