#What is this?
This project is a proof of concept for a live demo 2017 by Karl Tillström.
For more info, see http://www.sortingbits.net

In short; the project is fetching Bitcoin tickers from BitFinex, 
trains a neural network to predict the future price and runs a 
"Simulator" to test the predictions.

It uses the Encog library (http://www.heatonresearch.com/encog/) for the neural networks part 
and Eventstore (http://www.geteventstore.com) as data store.

#Want to learn this stuff for real?
Start with: https://www.coursera.org/learn/machine-learning


#Usage:
1. Download and install (run) Eventstore from http://www.geteventstore.com
2. Start TheScraper to fetch BTC tickers from BitFinex
3. Start TheTrainer to train a neural net using the fetched tickers (a good idea is to wait until you have a lot of tickers)
4. Start TheMoneyMaker to make predictions on the tickers (requires TheScraper to be running)

5. Rerun TheTrainer as many times as you wish, TheMoneyMaker updates to the latest bot

Feel like living on the edge? Add an integration with the BitFinex trading API instead of the Simulator.