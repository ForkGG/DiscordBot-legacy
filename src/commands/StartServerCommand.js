const { SlashCommandBuilder, EmbedBuilder } = require('discord.js')
const fs = require('fs');

class StartServerCommand {
    
    data = new SlashCommandBuilder()
        .setName('start')
        .setDescription('Starts the minecraft server with the given name')
        .addStringOption((opt) => opt
            .setName('name')
            .setDescription('Name of the server to start')
            .setRequired(true));

    async execute (interaction, syncs, iterations) {
        const server = interaction.options.getString('name', true);
        const issuer = interaction.user;

        let data = fs.readFileSync('config.json', 'utf8');
        data = JSON.parse(data);
        let token = data.guilds[interaction.guildId];
        let ws;
        if (token) {
            ws = syncs.get(token);
        }

        if (!ws || ws == null) {
            var embed = new EmbedBuilder()
                .setColor('#be2230')
                .setTitle('Not connected')
                .setAuthor({
                    name: 'Fork'
                })
                .setDescription(`Failed to start server ${server}. We are not connected to any Fork instance`);

            await interaction.reply({ embeds: [embed], ephemeral: true });
            return;
        }
        
        var embed = new EmbedBuilder()
            .setColor('#d3902d')
            .setTitle('Starting server')
            .setAuthor({
                name: 'Fork'
            })
            .setDescription(`Trying to start ${server}. Please wait`);

        const response = await interaction.reply({ embeds: [embed], ephemeral: true, fetchReply: true });
        iterations.set(response.id, interaction);

        ws.send(`start|${server}|${response.id}`);
    }
}

module.exports = StartServerCommand;