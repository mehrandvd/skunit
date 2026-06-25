using Microsoft.Agents.AI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.AI;

namespace Demo.MoodyChef.Console
{
    public static class AgentGallery
    {
        public static AIAgent CreateSloppyAgent(IChatClient chatClient)
        {
            return chatClient.AsAIAgent(
                instructions: """
                              You are a chef that serves food based on the mood of the user. 
                              You will just suggest food based on the mood of the user.

                              UserMood.NormalOrHappy => "Pizza, Pasta, Salad",
                              UserMood.Sad => "Ice Cream, Chocolate, Cake",
                              UserMood.Angry => "Nothing, you're on a diet",
                              _ => "I don't know what you want"
                              """
            );
        }


        public static AIAgent CreateToolBasedAgent(IChatClient chatClient)
        {
            return chatClient.AsAIAgent(
                instructions:
                """
                You are a chef that serves food based on the mood of the user. 
                You will just suggest food based on the mood of the user and don't suggest anything else.
                You have a tool called GetFoodMenu that takes in a UserMood and returns a food menu based on the attitude of the user.
                """,
                tools: 
                [
                    AIFunctionFactory.Create(GetFoodMenu)
                ]
            );
        }

        [Description("Returns the food menu based on the attitude of the user")]
        private static string GetFoodMenu(
            [Description("User's mood based on its chat.")]
            UserMood mood
        )
        {
            System.Console.WriteLine($"User mood: {mood}");
            
            return mood switch
            {
                UserMood.NormalOrHappy => "Pizza, Pasta, Salad",
                UserMood.Sad => "Ice Cream, Chocolate, Cake",
                UserMood.Angry => "Nothing, you're on a diet",
                _ => "I don't know what you want"
            };
        }

        public enum UserMood
        {
            NormalOrHappy,
            Sad,
            Angry
        }
    }
}
