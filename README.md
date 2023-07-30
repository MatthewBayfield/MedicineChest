# MedicineChest
## Overview
MedicineChest is a C# console application whose purpose is to implement and test a prototype lift operation algorithm, with the main aim of optimising the lift's efficiency.

##  Project Scenario

### The client requirements
MedicineChest is a company seeking a new lift for their office block, and as part of this are looking for a sotfware engineer to create a Lift algorithm to operate the lift.
They have high expectations for the lift with a strong desire for efficiency. To this end the following information was provided by the client to aid in the design of the lift algorithm.

- The office block has 10 floors.
- The people working in the block are equally distributed across all floors.
- People start arriving for work at around 8am, and everyone has left by 6pm.
- The lift will only have one external call button instead of two external (up and down) buttons.
- The lift will have a maximum capacity of 8 people.

## Algorithm Design
**For a complete and full thorough discussion of the algorithm design please see [lift_algorithm_design.pdf](lift_algorithm_design.pdf)** 

What follows is a summary or outline of the complete design process.

### Defining lift efficiency
The two primary measures of lift efficiency are from the perspective of the lift and the individual user. The lift perspective is a holistic measure of efficiency, in that it measures
the total time for the lift to move a fixed number of people between the floors they called the lift from, to their destination floors. This however ignores the user experience,
and the efficiency from the user perspective, as measured by the distribution of journey times for individual users of the lift. An algorithmm that optimises both forms of
efficiency is desired, but in reality there is likely a trade-off between the two to some extent, even though they will be strongly positively correlated. It has been decided to
prioritise minimising the user journey times when there is a conflict between the two measures of efficiency.

### Initial considerations and assumptions
- For most times of the day journeys to and from a given floor are equally likely relative to all other floors.
- At times when most people are arriving to work (8am), pretty much all lift journeys will be from the ground floor to the upper floors,
  and vice versa at the end of the work day (6pm).
- There is no way to fully incorporate the number of users travelling between two floors, and also the lift capacity in to the decisions made by a candidate algorithm.
- At best a probabilistic assumption can be made as to the most likely direction of desired travel for users calling the lift from a floor.
- The time taken to travel between adjacent floors is constant. Additionally it will be assumed that the time during which the lift stops at a floor is approximately constant.
- The algorithm will need to update the route taken by the lift dynamically in response to real-time unpredictable lift calls and floor selections made			
  by users boarding the lift. The optimal route will change with time. The lift will also have a minimum response time in order to update its route and also change its direction
  of travel.

### Strategy devised
#### In what order to travel to existing selected destination floors
In the absence of additional lift calls, the strategy for optimising the lifts route to the destination floors, as selected by the lift riders, is calculated by
considering all permuations of the floors treated as a sequence of integers. An individual route or path through the floors corresponds to a permutation of the integers 1-10.
For each permutation of floors or path, the total number of floors traversed can be calculated as the sum of the difference of adjacent terms in each permutation,
with the initial floor adjoined at the start. Using this the user journey time, for a user whose selected floor is the jth floor along the lifts path, can be calculated by
summing up all these numbers of floors traversed between each stop before the jth stop, multiplied by the time taken to travel between adjacent floors; by then adding a term
equal to the number of stops before the jth stop multiplied by the average stop time, you obtain the total user journey time for one of the possible paths. Summing over all the
individual user journey times, then gives you the total user journey time for a candidate lift path. By comparing the total user journey times for each path, the path with the minimum
time will be selected. If more than one minimum path exists, then a path which also minimises the total lift time is chosen. This total lift time can be calculated for each path using the
same calculation for user journey times, specifically by using the calculation for the individual user journey time to reach the final stop in the path.

#### How to handle users boarding the lift and adding new selected floors
To handle users boarding the lift and selecting new destination floors, in effect the optimal path will be recalculated with additional floors existing in all possible paths. This
strategy does treat newly boarded users the same as users who were already on the lift, which has the potential downside of increasing the journey times of existing lift riders, as
the recalculated route may deviate significantly from the original route leading to their stops. It is hoped that by deviating the route for the original yet reached stops, that through
sufficient reductions in the journey times of newly boarded users, the total user journey time for newly boarded users and existing lift riders is minimised.

To prevent too many successive route deviations affecting the same users, perhaps leading to unacceptable journey times for these users --- as well as the lift becoming too unpredictable ---
it was decided that after so many stops where the lift has not travelled to certain stops, the lift will proceed to travel exclusively to those stops that have yet to been reached for a
threshold number of stops, in an optimal way, before returning to its normal behaviour. The threshold was chosen to be 9 stops.
There is a possibility that even this behaviour could itself pose problems by in effect causing itself to become more common, thereby increasing journey times.

#### How to handle new lift calls made during the lifts operation
To handle new lift calls during the lifts operation, the algorithm needs to balance the journey times of users already on the lift with the waiting times of the users calling the lift,
whose journey time begins when waiting. If a user is made to wait excessively for the lift, regardless of how quickly their ride in the lift is, they will be dissatisfied. In contrast
adding extra stops/floors to a user's ride in the lift, will increase their journey times. It again should be emphasised that there is no way of knowing how many people are waiting for
the lift to arrive on a certain floor, or whether they have been waiting the same time, or which destinations floors they want, or even the direction they are heading. The strategy for
responding to lift calls depends on the scenario as follows:

1.  
- The lift departs the current stop. Before travelling to the next scheduled destination stop, a called floor must be passed.
- The subsequent destination floor after the next destination floor has the lift travelling in the same direction as it has been doing so.
- The waiting user has a stronger probability of going to a floor in the same direction.
- In this case it is fine to stop at the called floor, as there is a better chance the user is going to a next floor or a floor near to a next floor in the direction
  the lift is travelling. Their journey time and waiting will then be shorter, at only the initial cost of a single extra stop to other users.


2.  

- The lift departs the current stop. Before travelling to the next scheduled destination stop, a called floor must be passed.
- The subsequent destination floor after the next destination floor has the lift travelling in the same direction as it has been doing so.
- The waiting user has a stronger probability of going in the opposite direction.
- In this case it is better to not stop at the called floor, as there is a better chance the user is going to a floor not in the direction the lift is travelling.
  Their increased waiting time will be equal to their journey time if they were picked up; and a single extra stop for some other users will be removed.


It should be noted that relying on this probabilistic interpretation for predicting whether a user is going up or down will not work in each case, but for the
long term average it should. The probability will be assigned based on the number of floors above and below the called floor. For example if there are more floors above,
it will be taken that the user is more likely to be going up. This assumes as each floor is equally distributed, and in the absence of any other information, that there is
an equal chance of movement between floors. If there are an equal number of floors above and below, then the user will be assumed to be going down.

For called floors that are skipped, they will not be considered to be added to the route again until the next stop. If there are only floors going to be skipped or not passed-by, then all
the called floors will be treated as if they were a destination floor, and the route reoptimised using all selected floors and called floors existing at each stop.
Again like with destination floors, if a called floor has not been visited for 9 stops since called, it will be prioritised as was discussed previously.
Finally if a called floor is already a destination floor, or becomes a destination floor, then two identical contributions involving this floor will feature in the total user journey
time calculation for a path.

#### Taking in to account the lift capacity
The only way to tell that definitely 8 people are in the lift is if 8 floors have been selected. If this occurs then called floors will not be stopped at
since some people have disembarked at a destination floor, and fewer than 8 selected floors remain. One issue with this is that the lift could still be full even with fewer
than 8 selected floors, leading to the lift stopping at called floors, and users not being able to get on. To partially get around this issue,
the lift weight sensor (which lifts have for controlling the lift pulling force) could be used to detect total weight changes, which would indicate whether users boarded.
If no one boarded at a called stop, then the lift would be assumed to be full, and act as just described above. The called floor would not be removed from the set of floors to visit,
as it would need to be revisited.

#### Lift behaviour when empty
Through most times of the day, when empty, it makes sense for the lift to wait at floor 5 or 6, roughly half way, in that there are 5 and 4 floors either side.
As each floor is equally distributed in people, with assumed equal movement between floors, a call to the lift could happen above or below with equal probability,
At the start of the day at 8am, people begin arriving to work, and so for a certain time window starting then, it would make sense for the lift to wait at floor 1 (Ground floor).
Likewise at the end of the day, for a time window before 6pm, it would make sense for the lift to wait at floor 10, as most people will be heading down to the floor 1,
and the longest journey is from floor 10, with the potential to pick up users on the way down, also heading down.

### Algorithm outline

#### Global variables and events
There will need to exist globally accessible variables tracking the sets of selected destination floors, and called floors, that updated at each stop. There will also need to be
sets of variables that count the number of stops since a called or destination floor were called or selected. Finally there needs to be a global variable indicating the current floor
stopped at.
	
Events such as a user pressing a floor's lift call button, or users pressing buttons to select a floor as a destination, will need to be continuously monitored and used to update
in real time the selected and called floors.

#### A major function
A function is needed to calculate an optimal path using the set of selected destination floors passed as a parameter, and the set of called floors passed as a parameter. 
The function body should consist of steps like:

1. If there are selected floors, check whether their respective counters have exceeded the threshold number of stops; if so add the floor to a set recording the floors that have exceeded		
   the threshold number of visits since last being stopped at.
2. Repeat step 1, but for called floors.
3. If there are no called floors then:
	- If there are no threshold exceeded selected floors, calculate the total user journey time T(P) for each possible path involving the selected floors. Extract paths which minimise T(P);
	from these choose any path that minimises the total lift journey time (Q_n, where n = # of selected floors). Return calculated optimal path P.
	- If there are some threshold exceeded selected floors, then repeat 2. but using the threshold exceeeded selected floors only.
4. If there are called floors:
	- If there are no selected floors:
		* If there are no threshold exceeded called fllors, calculate T(P) for each possible path involving the called floors. Extract paths which minimise T(P);
		  from these choose any path that minimises Q_n, where n = # of called floors. Return calculated optimal path P.
		* If there are threshold exceeded called floors, calculate T(P) for each possible path involving the threshold exceeded called floors. Extract paths which minimise T(P);
		  from these choose any path that minimises Q_n, where n = # of threshold exceeded called floors. Return calculated optimal path P.
	- If there are selected floors:
		* If there are threshold exceeded selected floors and threshold exceeded called floors, then calculate T(P) for each possible path involving these threshold exceeded floors.
		  Extract paths which minimise T(P); from these choose any path that minimises Q_n, where n = # of threshold exceeded floors. Return calculated optimal path P.
		* If there threshold exceeded called floors only, calculate T(P) for each possible path involving these floors. Extract paths which minimise T(P);
		  from these choose any path that minimises Q_n, where n = # of threshold exceeded called floors. Return calculated optimal path P.
		* If there are only threshold exceeded selected floors only, then repeat the previous point, but for threshold exceeded selected floors.
		* If there are both selected and called threshold exceeded floors, calculate T(P) for each possible path involving the threshold exceeded selected floors.
		  Extract paths which minimise T(P); from these choose any path that minimises Q_n, where n = # of threshold exceeded floors. For this optimum path P, extract the subset
		  of threshold exceeded floors that must be passed-by. For this subset calculate whether the associated user on each floor has a stronger probability of going in the same
		  direction as the lift in P, as discussed in the strategy devised section. Then extract the subset of floors of this subset, for which waiting users do have a stronger
		  probability of going in the same direction as the lift. Of the floors left, if any, insert the first floor to be passed-by in P, in to P in its position between the two
		  consecutive floors in P. Return this P as the optimal path.	    
		  
		  If there are no suitable threshold exceeded floors, recall these will be treated as destination floors. Add them to an expanded floor set containing the threshold
		  exceeded selected floors. Then calculate T(P) for each possible path involving this expanded set. Extract paths which minimise T(P); from these choose any path that
		  minimises Q_n, where n = # of threshold exceeded called and selected floors. Return calculated optimal path P.
		* If there are only normal called and selected floors, repeat the previous few steps, replacing the threshold exceeded called and selected floors, with
		  the normal selected and called floors.

#### The main loop
The code will need to contain a main loop, that starts when the lift stops at a floor. It should include steps simillar to the following ones:
1. Arrived/waiting at current floor.
2. Check if lift is now empty.
3. If the lift is empty, then check the current time:
	- 7.30am < current time < 9.30am,  then go to or stay at floor 1.
	- current time > 5pm, then go to or stay at floor 10.
	- Else go to  or stay at floor 5. Lift will travel to or stay at floor, and so restart the loop with 1.
4. If the current floor is a called and or selected floor, open the lift doors.
5. If the current floor is a selected floor, then remove it from the list of selected floors, and reset the stop counter for the current floor as a selected floor to zero.
6. Increment by one all the stop counters for all remaining selected floors.
7. If the current floor is a called floor:
	- If total weight has changed, remove the current floor from the list of called floors. Reset the stop counter for the current floor as a called floor.
	- If total weight has not changed, indicate lift is full.
8. Increment the stop counter for all remaining called floors.
9. If doors open, allow time for users to board and disembark the lift. Close doors and wait 20 seconds or so for new users to select new floors if necessary.
10. Update the selected floors set.
11. Update the called floors set.
12. If there are no called or selected floors after the updates, check the current time, and go back to
	step 4.
13. If the lift is full indicate the lift is full.
14. If lift is full, call the major function to calulate the opitmal route path, when not stopping at called floors.
15. If lift is not full, calculate the optimal path using both selected and called floors.
16. Using the optimal path P, move the lift to the next floor in accordance with P. Once arrived, the current floor is updated and the loop restarts at 1.

## Current implementation of the algorithm prototype program
The current console application implements most of the algorithm as described in the design section. It additionally tests the algorithms performance by simulating the lifts operating
environment. It takes an input CSV file, obtained using a user prompt to input a valid file path.The format of this CSV file should match that found
in [lift_calls_input_csv.csv](lift_calls_input_csv.csv). The file provides simulated lift calls occuring at different times from different floors, from labelled callers, and with their
specified destination floors. The program also asks the user to provide a valid file directory, where the program creates a output CSV file containing the journey details of the lift
callers, as well as the lift itself, as recorded on a stop-by-stop basis. The content of the output CSV file gives an indication of how the algorithm behaves, as well as its performance.

A timer is started once the lift becomes operational, and when the calling time for a lift call in the input CSV file occurs, the call occurs in the program by updating the set of
called floors. The program also simulates the time taken to travel between stops, as well as the average time for a stop. The user is also given the option of a Quick mode whereby the
timers are sped up, as oppose to running in real-time; this will allow the user to observe the behaviour of the lift and performance of the algorithm faster. Various status updates
and other information regarding the algorithms performance and the lift behaviour are periodically output to the terminal. They allow the user to visualise the lifts response to handling
lift calls and users selecting destination floors. For example at every stop the time at which lift riders depart the lift, and lift callers board the lift is printed to the terminal.
Furthermore the floors users call from, as well as the floors each user selects to travel to, are printed to the console.

Some lift behaviours not implemented in the current prototype, include the lift weight load at each stop. There are also some small practical changes made to the algorithm design.
A major one being that when the number of floors in candidate optimal paths is greater than 5, only paths involving 5 floors --- chosen out of all the actual floors that
would normally feature in a path --- are present in the candidate optimal paths. This is because of the calculation time needed to calculate all possible paths, of which there are n!.
Therefore when n > 5, all permutations of all combinations of 5 floors are used, and this corresponds to a subset of all possible paths. This will in some cases lead to a suboptimal
path being chosen, but this cannot be avoided.




