﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TgStickerMakerBot.Models;
using TgStickerMakerBot.Models.CallbackModels;

namespace TgStickerMakerBot.Comands
{
    public interface IBotCommand
    {
        public Task HandleTextMessageAsync(Message message, UserState userState, CancellationToken cancellationToken);

        public Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, long userId, CancellationToken cancellationToken, UserState userState);

        Task StartAsync(UserState userState, CancellationToken cancellationToken);

        //public string GetCurrentCommandStep();
    }
}
