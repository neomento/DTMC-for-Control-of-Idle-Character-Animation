using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTMCDecision : StateMachineBehaviour
{
    const string NextStateParam = "Next SM State";

    int currentState;

    float[] probabilities = new float[] { .1f, .3f, .6f };

    public const float BasicStateStayTime = 10f; // Expected stay time of about 20s

    // Self transition
    public static float StayProbability(float state_probability, float length)
    {
        return Mathf.Exp(Mathf.Log(state_probability) / (BasicStateStayTime / length));
    }

    // Exit transition
    public static float EnterProbability(float source_probability, float target_probability, float length)
    {
        return (1f - StayProbability(source_probability, length)) * target_probability / (1f - source_probability);
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float length = stateInfo.length;

        currentState = animator.GetInteger(NextStateParam);

        int nextState = PickNextState(currentState, length, probabilities);
        animator.SetInteger(NextStateParam, nextState);
    }


    // Compute the leave probabilites (see the presentation CharacterBehaviour for details)
    int PickNextState(int currentState, float stateLength, float[] probabilities)
    {
        int newState = 0;

        float[] updatedProbabilites = new float[probabilities.Length];

        for (int i = 0; i < updatedProbabilites.Length; i++)
        {
            if (i == currentState)
            {
                updatedProbabilites[i] = StayProbability(probabilities[i], stateLength);
            }
            else
            {
                updatedProbabilites[i] = EnterProbability(probabilities[currentState], probabilities[i], stateLength);
            }
        }
        // Convert to cumulative
        for (int i = 1; i < updatedProbabilites.Length; i++)
        {
            updatedProbabilites[i] = updatedProbabilites[i - 1] + updatedProbabilites[i];
        }

        // If the cumulative probability is too low then 0 is selected
        float randomVal = UnityEngine.Random.Range(0f, 1f);
        // Select when fits to the cumulative range (random selection)
        for (int i = 0; i < updatedProbabilites.Length; i++)
        {
            if (randomVal < updatedProbabilites[i])
            {
                newState = i;
                break;
            }
        }

        return newState;
    }
}
