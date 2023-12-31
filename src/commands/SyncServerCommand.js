const { SlashCommandBuilder, EmbedBuilder, PermissionFlagsBits } = require('discord.js');
const fs = require('fs');

class SyncServerCommand {
    
    data = new SlashCommandBuilder()
        .setName('sync')
        .setDescription('Synchronizes the server with the fork instance')
        .addStringOption((opt) => opt
            .setName('token')
            .setDescription('The fork generated token')
            .setRequired(true))
        .setDefaultMemberPermissions(PermissionFlagsBits.Administrator);

    async execute (interaction, syncs, iterations, sockets) {
        const token = interaction.options.getString('token', true);
        const issuer = interaction.user;

        if (syncs.get(token)) {
            embed = new EmbedBuilder()
                .setColor('#be2230')
                .setTitle('Already synchronized')
                .setAuthor({
                    name: 'Fork'
                })
                .setDescription('We are already synchronized with Fork');
            return;
        }

        let data = fs.readFileSync('config.json', 'utf8');
        let embed;
        try {
            let json = JSON.parse(data);
            json.guilds[interaction.guildId] = token;

            fs.writeFileSync('config.json', JSON.stringify(json));
            embed = new EmbedBuilder()
                .setColor('#72cc72')
                .setTitle('Synchronized')
                .setAuthor({
                    name: 'Fork'
                })
                .setDescription('Success! Your discord server and your fork instance are now connected. Thanks for using Fork');

            let ws = sockets.get(token);
            if (ws) {
                console.info('Noticing websocket about connection...')
                ws.send(`status|linked|${interaction.guild.name}`)
                syncs.set(token, ws);
            }
        } catch (error) {
            embed = new EmbedBuilder()
                .setColor('#be2230')
                .setTitle('Not synchronized')
                .setAuthor({
                    name: 'Fork'
                })
                .setDescription('There was a problem while synchronizing your server and your fork instance');
        }

        await interaction.reply({ embeds: [embed], ephemeral: true });
    }
}

module.exports = SyncServerCommand;