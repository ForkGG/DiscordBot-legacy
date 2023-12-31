const { SlashCommandBuilder, EmbedBuilder } = require('discord.js')
const fs = require('fs');

class StopServerCommand {
    
    data = new SlashCommandBuilder()
        .setName('stop')
        .setDescription('Stops the minecraft server with the given name')
        .addStringOption((opt) => opt
            .setName('name')
            .setDescription('Name of the server to stop')
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
                .setDescription(`Failed to stop server ${server}. We are not connected to any Fork instance`);

            await interaction.reply({ embeds: [embed], ephemeral: true });
            return;
        }
        
        var embed = new EmbedBuilder()
            .setColor('#d3902d')
            .setTitle('Stopping server')
            .setAuthor({
                name: 'Fork'
            })
            .setDescription(`Trying to stop ${server}. Please wait`);

        const response = await interaction.reply({ embeds: [embed], ephemeral: true, fetchReply: true });
        iterations.set(response.id, interaction);

        ws.send(`stop|${server}|${response.id}`);
    }
}

module.exports = StopServerCommand;