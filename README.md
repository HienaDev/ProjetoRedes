# Jogo por Turnos - Redes

## Autoria

- António Rodrigues, 22202884

## Introdução

Este jogo é um jogo de tiros jogado por turnos, numa grelha, em que cada jogador escolhe no seu turno se quer andar e disparar.
O jogo usa a package [Unity Netcode for GameObjects](https://docs.unity3d.com/Manual/com.unity.netcode.gameobjects.html) e a package [Relay](https://docs.unity3d.com/Manual/com.unity.services.relay.html) também do Unity.
Usa como base o código feito em aula com o Professor Diogo Andrade.

## Descrição Técnica

### Diagrama de Redes

O sistema de relay do unity é um sistema peer to peer, logo o host para além de ser o server também é um cliente, no casso do nosso jogo, o server não tem acesso a nenhum dos jogadores, logo acaba por atuar como um servidor e nunca como um cliente.

![Diagrama de redes](./Images/diagrama_de_redes.png)

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

#### NetworkSetup

Dentro do NetworkManager temos o Network Setup que é onde tudo é inicializado:

Temos dois métodos iniciais, o StartServer e o StartPlayer, este métodos são chamados com os botões Start Server e Start Player, respetivamente.
Em ambos os métodos inicializamos uma lista de *Player* para guardar as sprites que são dispostas durante o jogo.
Depois verificamos se o prótocolo do UnityTransport é um relay server, e se for, tornamos a variável isRelay verdadeira.
No final desligamos a interface do menu que inclui os botões e ativamos a interface com a mensagem de "Loading...":

![Metodos](./Images/metodos_iniciais.png)

Depois temos o método StartAsServerCR, que é chamado no método StartServer, neste método conectamo-nos ao relay caso o booleano isRelay esteja em verdadeiro, e é ai que recebemos o código para os jogadores se conectarem.
Iniciamos o servidor, e caso o servidor seja iniciado instanciamos o objeto GameManager, que vai controlar todo o jogo e mexer os sprites dos jogadores, instanciar as balas, detetar colisões e controlar o tempo e os turnos.
Por fim desligamos a interface de Loading, e ativamos a interface com o botão de começar mais cedo, e a interface do jogo:

![Metodo Server](./Images/server_metodo.png)

No método StartAsClientCR, que é chamado no método StartPlayer, neste método, tal como no do servidor, conectamos ao relay, depois iniciamos o servidor e caso seja iniciado, ativamos a interface de jogo e delisgamos a interface de Loading:

![Metodo Cliente](./Images/metodo_client.png)

Também inicializamos o método *OnClientConnected* que é controlado pelo NetworkManager no servidor.
Neste método verificamos os pontos iniciais por um que não tenha nenhum jogador instanciado por perto, depois disso instanciamos o sprite do jogador, e inicializamo-lo na rede com a sua componente *NetworkObject*.
Depois intanciamos o controlador do jogador, inicializamo-lo na rede com a sua componente *NetworkObject* e damos a posse dele ao cliente:

![OnClientConnect](./Images/delegate_connecting.png)
![OnClientConnect](./Images/player_connecting.png)

#### GameManager

No *script* GameManager é onde todo o jogo acontece.
Durante o Update verificamos quantos jogadores estão vivos:

![Jogadores vivos](./Images/players_alive.png)

Começamos o jogo e mudamos o tamanho da grelha dependendo do número de jogadores:
Aqui atualizamos o temporiazdor do lado do servidor, e também do lado dos jogares usando um método *ClientRpc - UpdatePlayersClientRpc*.

![Grid and start](./Images/player_and_grid_start.png)

Atualizamos a posição de todos os jogadores e balas a cada *timerTurn* segundos, caso o jogo tenha começado:

![PlayerUpdate](./Images/player_update.png)

No método *UpdatePlayers* apenas atualizamos as balas e os jogadors do lado do servidor, como estes objetos têm o componente *ClientNetworkTransform*, eles são atualizados automaticamente do lado do cliente:

![PlayerUpdate](./Images/method_update_player.png)

No método MovePlayer, verificamos o último input que temos do jogador, guardado no dicionário players, e guardamos na posição *newPos*, depois verificamos se o último move foi *shoot*, caso tenha sido, instanciamos uma bala, senão, verificamos se o seu ultimo movimento é valido no método *CheckIfInGrid* que verifica se o jogador se tentou mover para fora da grelha, caso o movimento seja dentro da grelha, atualizamos a posição do jogador:

![PlayerUpdate](./Images/check_valid_player.png)

Fazemos o mesmo para a bala, mas caso esteja fora da grelha, destruímos a bala:

![BulletMove](./Images/bullet_move.png)

Caso o movimento da bala seja válido, verificamos se houve colisão com um jogador, caso haja, destruímos esse jogador e a bala também:

![CheckDeath](./Images/check_for_death.png)

#### PlayerController

O script *PlayerController* é o unico script que o cliente controla, neste script associamos o controlador a um jogador, e caso sejamos o jogador local, atualizamos o UI com o icon do jogador e com a ação nula.
Depois registamos o play no gameManager, para podermos depois enviar para o gameManager as ações que o cliente faz.

![PlayerControllerStart](./Images/player_controller_start.png)

Durate o update, verificamos os inputs do jogador local, atualizamos o UI respetivamente e registamos esse movimento no gameManager:

![PlayerControllerUpdate](./Images/player_controller_update.png)

### Largura de Banda

Quando um cliente se conecta ao servidor, envia-se cerca de 550 bytes e recebe-se 112 + 98 bytes:

![Player Connect](./Images/profiler_player_connect_1.png)
![Player Connect](./Images/profiler_player_connect_2.png)

O servidor envia 24 bytes por cada sincronização do tempo:

![TimeSync](./Images/time_sync.png)

Quando o jogo começa, é enviado cerca de 45 bytes para cada jogador por frame, os picos que se vêm são os 24 bytes da sincronização de tempo:

![Bytes per Frame](./Images/bytes_por_frame.png)

Sempre que um jogador envia um input ao servidor, o servidor recebe cerca de 31 bytes por cada cliente, e envia cerca de 13 bytes por cliente:

![Bytes per input](./Images/bytes_per_input_1.png)
![Bytes per input](./Images/bytes_per_input_2.png)

Por cada turno (a cada *timerTurn* seconds), são enviados 90 + 39 bytes para cada cliente por cada bala, neste caso foram criadas duas balas, depois são enviados 18 bytes para cada movimento da bala para cada cliente, durante este turno haviam 6 balas:

![Bytes per turn](./Images/bytes_per_turn_1.png)
![Bytes per turn](./Images/bytes_per_turn_2.png)

Quando um jogador morre são enviados cerca de 224 bytes para cada jogador:

![Bytes per death](./Images/bytes_per_death.png)

## Links

[Repositório Git Hub](https://github.com/HienaDev/ProjetoRedes)

Para testar a build, uma pessoa terá que executar o programa duas vezes, uma para ligar o servidor e outro para jogar, depois qualquer pessoa com o executável e com o código pode juntar-se a partida:
[Build Projeto](https://drive.google.com/file/d/1O4kKrpXxxA3TtClZaGw47pU3MQbqnPh4/view?usp=sharing)

## Agradecimentos

Agradeço ao Professor Diogo por ter disponibilizado o projeto feito em aula para usar como base para este projeto.
