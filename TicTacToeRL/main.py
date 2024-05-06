import time
import random

from environment import Environment
import tensorflow as tf
import numpy as np
from tf_agents.specs import array_spec
from tf_agents.environments import tf_environment
from tf_agents.environments import tf_py_environment
from tf_agents.environments import utils
from tf_agents.networks import categorical_q_network
from tf_agents.agents.categorical_dqn import categorical_dqn_agent
from tf_agents.drivers import dynamic_step_driver
from tf_agents.environments import suite_gym
from tf_agents.environments import tf_py_environment
from tf_agents.eval import metric_utils
from tf_agents.metrics import tf_metrics
from tf_agents.networks import categorical_q_network
from tf_agents.policies import random_tf_policy
from tf_agents.replay_buffers import tf_uniform_replay_buffer
from tf_agents.trajectories import trajectory
from tf_agents.trajectories import StepType
from tf_agents.utils import common
from matplotlib import pyplot as plt

py_env = Environment()
utils.validate_py_environment(py_env, episodes=5)
tf_env = tf_py_environment.TFPyEnvironment(py_env)

num_iterations = 1000  # @param {type:"integer"}

initial_collect_steps = 1000  # @param {type:"integer"}
collect_steps_per_iteration = 1  # @param {type:"integer"}
replay_buffer_capacity = 100000  # @param {type:"integer"}

fc_layer_params = (384, 256)

batch_size = 80  # @param {type:"integer"}
learning_rate = .01  # @param {type:"number"}
gamma = 0.55
log_interval = 1  # @param {type:"integer"}

num_atoms = 51  # @param {type:"integer"}
min_q_value = 0  # @param {type:"integer"}
max_q_value = 8  # @param {type:"integer"}
n_step_update = 2  # @param {type:"integer"}

num_eval_episodes = 10  # @param {type:"integer"}
eval_interval = 10  # @param {type:"integer"}

num_agents = 2

agent_list = []

for _ in range(num_agents):

    categorical_q_net = categorical_q_network.CategoricalQNetwork(
        tf_env.observation_spec(),
        tf_env.action_spec(),
        num_atoms=num_atoms,
        fc_layer_params=fc_layer_params)


    optimizer = tf.compat.v1.train.AdamOptimizer(learning_rate=learning_rate)

    train_step_counter = tf.Variable(0)


    def observation_and_action_constraint_splitter(observation):
        mask = [1, 1, 1, 1, 1, 1, 1, 1, 1]
        try:
            index = 0
            #for num in observation.numpy()[0][0]:
            for num in py_env._state:
                if num != 0:
                    mask[index] = 0
                index += 1

            # print(observation)
            # print(mask)
            final_mask = tf.convert_to_tensor([mask], np.int32)
            #print(final_mask)
            return observation, final_mask
        except:
            return observation, observation


    agent = categorical_dqn_agent.CategoricalDqnAgent(
        tf_env.time_step_spec(),
        tf_env.action_spec(),
        categorical_q_network=categorical_q_net,
        optimizer=optimizer,
        observation_and_action_constraint_splitter=observation_and_action_constraint_splitter,
        min_q_value=min_q_value,
        max_q_value=max_q_value,
        n_step_update=n_step_update,
        td_errors_loss_fn=common.element_wise_squared_loss,
        gamma=gamma,
        train_step_counter=train_step_counter)

    agent.initialize()

    agent_list.append(agent)


def compute_avg_return(environment, policy, agent, num_episodes=10):
    total_return = 0.0
    for _ in range(num_episodes):

        time_step = environment.reset()
        episode_return = 0.0

        while not time_step.is_last():
            action_step = policy.action(time_step)
            py_env._current_agent = agent
            time_step = environment.step(action_step.action)
            episode_return += time_step.reward

            randomNum = random.randint(0, 8)
            while py_env._state[randomNum] != 0 and not time_step.is_last():
                randomNum = random.randint(0, 8)
            py_env._state[randomNum] = 2
        total_return += episode_return

    avg_return = total_return / num_episodes
    return avg_return.numpy()[0]


random_policy = random_tf_policy.RandomTFPolicy(tf_env.time_step_spec(),
                                                tf_env.action_spec(),
                                                observation_and_action_constraint_splitter=
                                                observation_and_action_constraint_splitter)

compute_avg_return(tf_env, random_policy, 0, num_eval_episodes)

replay_buffer = tf_uniform_replay_buffer.TFUniformReplayBuffer(
    data_spec=agent_list[0].collect_data_spec,
    batch_size=tf_env.batch_size,
    max_length=replay_buffer_capacity)

replay_buffer_2 = tf_uniform_replay_buffer.TFUniformReplayBuffer(
    data_spec=agent_list[1].collect_data_spec,
    batch_size=tf_env.batch_size,
    max_length=replay_buffer_capacity)


def collect_step(environment, policy, agent):
    time_step = environment.current_time_step()
    action_step = policy.action(time_step)
    py_env._current_agent = agent
    next_time_step = environment.step(action_step.action)
    traj = trajectory.from_transition(time_step, action_step, next_time_step)

    # Add trajectory to the replay buffer
    if agent == 0:
        replay_buffer.add_batch(traj)
    else:
        replay_buffer_2.add_batch(traj)


def Print_Board():
    count = 0
    # print(tf_env.current_time_step())
    # for num in tf_env.current_time_step().observation.numpy()[0][0]:
    # print(py_env._state)
    for num in py_env._state:
        if num == 1:
            print('X|', end="")
        elif num == 2:
            print('O|', end="")
        else:
            print(' |', end="")

        count += 1

        if count == 3:
            print('')
            print('-+-+-')
            count = 0

    print('\n\n')


for _ in range(initial_collect_steps):
    collect_step(tf_env, random_policy, 0)

# This loop is so common in RL, that we provide standard implementations of
# these. For more details see the drivers module.

# Dataset generates trajectories with shape [BxTx...] where
# T = n_step_update + 1.
dataset = replay_buffer.as_dataset(
    num_parallel_calls=3, sample_batch_size=batch_size,
    num_steps=n_step_update + 1).prefetch(3)

dataset_2 = replay_buffer_2.as_dataset(
    num_parallel_calls=3, sample_batch_size=batch_size,
    num_steps=n_step_update + 1).prefetch(3)

iterator = iter(dataset)
iterator_2 = iter(dataset_2)

# Reset the train step
agent_list[0].train_step_counter.assign(0)
agent_list[1].train_step_counter.assign(0)

# Evaluate the agent's policy once before training.
avg_return = compute_avg_return(tf_env, agent_list[0].policy,0, num_eval_episodes)
avg_return_2 = compute_avg_return(tf_env, agent_list[1].policy,1, num_eval_episodes)
returns = [avg_return, avg_return_2]
losses = []
player_turn = True

tf_env.reset()

count = 0

for _ in range(num_iterations):
    py_env._true_start = True
    count += 1
    # print(tf_env.current_time_step().step_type)
    while not tf_env.current_time_step().step_type == StepType.LAST:
        collect_step(tf_env, agent_list[0].collect_policy, 0)

        player_action = random.randint(0, 8)
        if count < .975 * num_iterations and not tf_env.current_time_step().step_type == StepType.LAST:
            collect_step(tf_env, agent_list[1].collect_policy, 1)
            # while py_env._state[player_action] != 0 and not tf_env.current_time_step().step_type == StepType.LAST:
            #     player_action = random.randint(0, 8)
            # py_env._state[int(player_action)] = 2
        elif not tf_env.current_time_step().step_type == StepType.LAST:
            Print_Board()
            player_action = input("Where play?")
            py_env._state[int(player_action)] = 2
    # print('Reset')

    tf_env.reset()

    # Collect a few steps using collect_policy and save to the replay buffer.
    # for _ in range(collect_steps_per_iteration):

    # Sample a batch of data from the buffer and update the agent's network.
    experience, unused_info = next(iterator)
    train_loss = agent_list[0].train(experience)

    experience_2, unused_info_2 = next(iterator_2)
    train_loss_2 = agent_list[1].train(experience)

    step = agent_list[0].train_step_counter.numpy()

    if step % log_interval == 0:
        print('0: step = {0}: loss = {1}'.format(step, train_loss.loss))
        print('1: step = {0}: loss = {1}'.format(step, train_loss_2.loss))
        losses.append(train_loss.loss)

    if step % eval_interval == 0:
        avg_return = compute_avg_return(tf_env, agent_list[0].policy, 0, num_eval_episodes)
        print('0: step = {0}: Average Return = {1:.2f}'.format(step, avg_return))
        returns.append(avg_return)

for ret in returns:
    print(ret)

for loss in losses:
    print(loss.numpy())


def plot_curve(list):
    """Plot a curve of one or more classification metrics vs. epoch."""

    plt.figure()
    plt.xlabel("Epoch")
    plt.ylabel("Value")

    x_axis = []
    for m in range(len(list)):
        x_axis.append(m)

    plt.plot(x_axis[1:], list[1:])

    plt.legend()

    plt.show()


plot_curve(losses)

print(py_env._player_victory)
print(py_env._computer_victory)
print(py_env._ties)
print(py_env._invalid_moves)
