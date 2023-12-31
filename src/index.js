require('dotenv').config();


const { Client, REST, Routes, Events, ActivityType, IntentsBitField, Collection, EmbedBuilder } = require('discord.js');
const WebSocket = require('ws');
const https = require('https');
const http = require('http');
const fs = require('fs');
const path = require('path');

const token = process.env.DISCORD_BOT_TOKEN;
const sockets = new Map();
const syncs = new Map();
const iterations = new Map();

const client = new Client({
    intents: [
        IntentsBitField.Flags.Guilds,
        IntentsBitField.Flags.GuildMessages,
        IntentsBitField.Flags.GuildMessageReactions,
        IntentsBitField.Flags.GuildMembers
    ]
});
const rest = new REST({
    version: 9
}).setToken(token);

const certPath = process.env.SSL_CERT_PATH;
let server;
if (fs.existsSync(certPath)) {
    server = https.createServer({
        pfx: fs.readFileSync(process.env.SSL_CERT_PATH),
        passphrase: process.env.SSL_CERT_PASS
    });
} else {
    server = http.createServer();
}

const ws = new WebSocket.Server({server});


client.once(Events.ClientReady, function(c) {
    console.log('Fork - Bot Loaded - ', c.user.tag);

	client.user.setPresence({
		activities: [
			{
				name: "Fork", 
				type: ActivityType.Playing 
			}
		],
		status: 'online',
	});

    ws.on('connection', (connection) => {
        connection.on('message', async(message) => {
            if (!message) return;
    
            message = Buffer.from(message).toString('utf8');
            if (!message.includes('|')) {
                return;
            }
            console.info(message);

            let messageData = message.split('|');
            let code = messageData[0];
    
            switch (code) {
                case 'login':
                    let providedToken = messageData[1];
                    let data = fs.readFileSync('config.json', 'utf8');
                    try {
                        data = JSON.parse(data);
                        for (const key in data.guilds) {
                            let guild = client.guilds.cache.get(key);
                            if (!guild) continue;

                            if (data.guilds[key] == providedToken) {
                                syncs.set(providedToken, connection);
                                connection.send(`status|linked|${guild.name}`)
                                connection.send('serverList|');
                                connection.send('playerList|');
                            }
                        }
    
                        sockets.set(providedToken, connection);
                    } catch (err) {
                        console.info(err);
                    }
                    break;
                case 'notify':
                    let guildId = () => {
                        let token;
                        sockets.forEach((value, key) => {
                            if (value == connection) token = key;
                        });

                        let k;
                        let data = fs.readFileSync('config.json', 'utf8');
                        data = JSON.parse(data);

                        for (const key in data.guilds) {
                            let guild = client.guilds.cache.get(key);
                            if (!guild) continue;

                            if (data.guilds[key] == token) {
                                k = key;
                            }
                        }

                        return k;
                    }
                    let gId = guildId();
                    let guild = client.guilds.cache.get(gId);
                    if (!guild) return;

                    let serverName = messageData[1];
                    let messageId = messageData[2];
                    let action = messageData[3];
                    let resultCode = messageData[4];

                    let interaction = iterations.get(messageId);
                    //let message = await channel.messages.fetch(messageId);

                    var embed;
                    switch (resultCode) {
                        case "44":
                            embed = new EmbedBuilder()
                                .setColor('#be2230')
                                .setTitle('Not started')
                                .setAuthor({
                                    name: 'Fork'
                                })
                                .setDescription(`Failed to start server ${serverName} because it does not exist`);
                            break;
                        case "40":
                            embed = new EmbedBuilder()
                                .setColor('#be2230')
                                .setTitle('Not started')
                                .setAuthor({
                                    name: 'Fork'
                                });

                            if (action == "start") {
                                embed.setDescription(`Failed to start server ${serverName}. Probably it is already started`);
                            } else {
                                embed.setDescription(`Failed to stop server ${serverName}. Probably it is not started`);
                            }
                            break
                        case "20":
                            embed = new EmbedBuilder()
                                .setColor('#72cc72')
                                .setTitle('Started')
                                .setAuthor({
                                    name: 'Fork'
                                });

                                if (action == "start") {
                                    embed.setDescription(`Sending start task to server ${serverName}.`);
                                } else {
                                    embed.setDescription(`Sending kill task to server ${serverName}.`);
                                }
                            break;
                        case "21":
                            embed = new EmbedBuilder()
                                .setColor('#72cc72')
                                .setTitle('Started')
                                .setAuthor({
                                    name: 'Fork'
                                });

                                if (action == "start") {
                                    embed.setDescription(`Successfully started server ${serverName}. It might take a few minutes to completely start`);
                                } else {
                                    embed.setDescription(`Successfully stopped server ${serverName}. It might take a few seconds to complete`);
                                }
                            break;
                    }

                    if (resultCode != "20") {
                        iterations.delete(messageId);
                    }
                    return await interaction.editReply({ embeds: [embed], ephemeral: true });
            }
        });
    });

    server.listen(8181, () => {
        console.info('Successfully started Fork websocket. SSL:', fs.existsSync(certPath))
    });
});

client.on(Events.InteractionCreate, async interaction => {
	if (interaction.isChatInputCommand()) {
		const command = interaction.client.commands.get(interaction.commandName);

		if (!command) {
			console.error(`No command matching ${interaction.commandName} was found.`);
			return;
		}

		try {
			await command.execute(interaction, syncs, iterations, sockets);
		} catch (error) {
			console.error(error);
			await interaction.reply({ content: 'There was an error while executing this command!', ephemeral: true });
		}
	}
});

const commands = [];
client.commands = new Collection();

console.info('Loading commands.');
const commandsPath = path.join(__dirname, 'commands');
const commandFiles = fs.readdirSync(commandsPath).filter(file => file.endsWith('.js'));

for (const file of commandFiles) {
	const filePath = path.join(commandsPath, file);
	const command = require(filePath);
    let instance = new command();

	if ('data' in instance && 'execute' in instance) {
		if (instance.setClient != undefined) {
			instance.setClient(client);
		}
		
		client.commands.set(instance.data.name, instance);
        commands.push(instance.data.toJSON());
	} else {
		console.warn(`The command at ${filePath} is missing a required "data" or "execute" property.`);
	}
}

client.login(token);

(async () => {
	try {
		console.log(`Started refreshing ${commands.length} application (/) commands.`);

		const data = await rest.put(
			Routes.applicationCommands(process.env.DISCORD_BOT_ID),
			{ body: commands },
		);

		console.log(`Successfully reloaded ${data.length} application (/) commands.`);
	} catch (error) {
		console.error(error);
	}
})();